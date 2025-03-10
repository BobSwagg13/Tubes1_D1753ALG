using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class WaveSurfingBot : Bot
{   
    // Targeting and tracking variables
    private int turnDirection = 1;
    private int lockedTargetId = -1;
    private int lostTargetCount = 0;
    private const int maxLostTargetCount = 5;
    private double enemyEnergy = 100;

    // Wave surfing constants
    private const double WALL_MARGIN = 30;
    private const double WALL_STICK = 160;
    private const int BINS = 47;
    private const double LESS_THAN_HALF_PI = 1.25; // Math.PI/2 would be perpendicular movement
    
    // Wave surfing variables
    private double[] surfStats = new double[BINS];
    private List<EnemyWave> enemyWaves = new List<EnemyWave>();
    private List<int> surfDirections = new List<int>();
    private List<double> surfAbsBearings = new List<double>();

    // Position tracking
    private Vector2 myLocation;
    private Vector2 enemyLocation;
    private double arenaWidth;
    private double arenaHeight;

    static void Main(string[] args)
    {
        new WaveSurfingBot().Start();
    }

    WaveSurfingBot() : base(BotInfo.FromFile("WaveSurfingBot.json")) { }

    public override void Run()
    {
        // Set bot colors
        BodyColor = Color.DodgerBlue;
        GunColor = Color.Black;
        RadarColor = Color.Red;
        BulletColor = Color.Yellow;
        
        // Initialize arena dimensions
        arenaWidth = ArenaWidth;
        arenaHeight = ArenaHeight;

        while (IsRunning)
        {
            // Keep radar spinning when idle
            if (RadarTurnRemaining == 0)
            {
                SetTurnRadarRight(Double.PositiveInfinity);
            }
            
            // Update position and waves
            UpdatePosition();
            UpdateWaves();
            DoSurfing();
            
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        UpdatePosition();
        
        // Calculate lateral velocity and bearing
        double lateralVelocity = Speed * Math.Sin(ToRadians(BearingTo(e.X, e.Y)));
        double absBearing = ToRadians(Direction + BearingTo(e.X, e.Y));

        // Radar lock
        double radarTurn = NormalizeRelativeAngle(absBearing - ToRadians(RadarDirection)) * 2.0;
        SetTurnRadarRight(ToDegrees(radarTurn));

        // Keep track of surfing direction
        surfDirections.Insert(0, (lateralVelocity >= 0) ? 1 : -1);
        surfAbsBearings.Insert(0, absBearing + Math.PI);

        // Wave detection
        double bulletPower = enemyEnergy - e.Energy;
        if (bulletPower < 3.01 && bulletPower > 0.09 && surfDirections.Count > 2)
        {
            EnemyWave enemyWave = new EnemyWave
            {
                FireTime = Time + 1,
                BulletVelocity = BulletVelocity(bulletPower),
                DistanceTraveled = BulletVelocity(bulletPower),
                Direction = surfDirections[2],
                DirectAngle = surfAbsBearings[2],
                FireLocation = enemyLocation
            };
            enemyWaves.Add(enemyWave);
        }

        // Update enemy energy
        enemyEnergy = e.Energy;

        // Update enemy location
        enemyLocation = Project(myLocation, absBearing, e.Distance);

        // Lock on to this target
        if (lockedTargetId == -1 || lockedTargetId == e.ScannedBotId)
        {
            lockedTargetId = e.ScannedBotId;
            lostTargetCount = 0;
            
            // Aim gun at target
            AimAtTarget(e.X, e.Y);
            
            // Fire when gun is ready
            if (GunHeat == 0)
            {
                double distance = DistanceTo(e.X, e.Y);
                if (distance < 200)
                    Fire(3);
                else if (distance < 400)
                    Fire(2);
                else
                    Fire(1);
            }
        }
        else
        {
            lostTargetCount++;
            if (lostTargetCount >= maxLostTargetCount)
            {
                lockedTargetId = -1;
                SetTurnRadarRight(Double.PositiveInfinity);
            }
        }
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Find the wave that hit us
        if (enemyWaves.Count > 0)
        {
            Vector2 hitBulletLocation = new Vector2((float)e.X, (float)e.Y);
            EnemyWave hitWave = null;

            // Find the wave that hit us
            foreach (EnemyWave wave in enemyWaves)
            {
                double distance = Vector2.Distance(myLocation, wave.FireLocation);
                if (Math.Abs(wave.DistanceTraveled - distance) < 50 &&
                    Math.Abs(BulletVelocity(e.Power) - wave.BulletVelocity) < 0.001)
                {
                    hitWave = wave;
                    break;
                }
            }

            // Log the hit for statistical analysis
            if (hitWave != null)
            {
                LogHit(hitWave, hitBulletLocation);
                enemyWaves.Remove(hitWave);
            }
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        AimAtTarget(e.X, e.Y);
        Fire(3);
        
        // Back up after collision
        SetForward(-100);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Hit wall! Adjusting course.");
        
        // Use wall smoothing to recover
        UpdatePosition();
        double angle = WallSmoothing(myLocation, ToRadians(Direction) + Math.PI / 2, -1);
        SetBackAsFront(ToDegrees(angle));
    }

    private void UpdatePosition()
    {
        myLocation = new Vector2((float)X, (float)Y);
    }

    private void UpdateWaves()
    {
        for (int i = 0; i < enemyWaves.Count; i++)
        {
            EnemyWave wave = enemyWaves[i];
            wave.DistanceTraveled = (Time - wave.FireTime) * wave.BulletVelocity;
            
            // Remove old waves
            if (wave.DistanceTraveled > Vector2.Distance(myLocation, wave.FireLocation) + 50)
            {
                enemyWaves.RemoveAt(i);
                i--;
            }
        }
    }

    private void DoSurfing()
    {
        EnemyWave surfWave = GetClosestSurfableWave();

        if (surfWave == null)
            return;

        double dangerLeft = CheckDanger(surfWave, -1);
        double dangerRight = CheckDanger(surfWave, 1);

        double goAngle = AbsoluteBearing(surfWave.FireLocation, myLocation);
        if (dangerLeft < dangerRight)
        {
            goAngle = WallSmoothing(myLocation, goAngle - LESS_THAN_HALF_PI, -1);
        }
        else
        {
            goAngle = WallSmoothing(myLocation, goAngle + LESS_THAN_HALF_PI, 1);
        }

        SetBackAsFront(ToDegrees(goAngle));
    }

    private EnemyWave GetClosestSurfableWave()
    {
        double closestDistance = 50000; // Very large distance
        EnemyWave surfWave = null;

        foreach (EnemyWave wave in enemyWaves)
        {
            double distance = Vector2.Distance(myLocation, wave.FireLocation) - wave.DistanceTraveled;

            if (distance > wave.BulletVelocity && distance < closestDistance)
            {
                surfWave = wave;
                closestDistance = distance;
            }
        }

        return surfWave;
    }

    private double CheckDanger(EnemyWave wave, int direction)
    {
        int index = GetFactorIndex(wave, PredictPosition(wave, direction));
        return surfStats[index];
    }

    private Vector2 PredictPosition(EnemyWave wave, int direction)
    {
        Vector2 predictedPosition = myLocation;
        double predictedVelocity = Speed;
        double predictedHeading = ToRadians(Direction);
        double maxTurning, moveAngle, moveDir;

        int counter = 0; // number of ticks in the future
        bool intercepted = false;

        while (!intercepted && counter < 500)
        {
            moveAngle = WallSmoothing(
                predictedPosition,
                AbsoluteBearing(wave.FireLocation, predictedPosition) + (direction * (Math.PI / 2)),
                direction) - predictedHeading;
            moveDir = 1;

            if (Math.Cos(moveAngle) < 0)
            {
                moveAngle += Math.PI;
                moveDir = -1;
            }

            moveAngle = NormalizeRelativeAngle(moveAngle);

            // maxTurning is built in like this, you can't turn more than this in one tick
            maxTurning = Math.PI / 720d * (40d - 3d * Math.Abs(predictedVelocity));
            predictedHeading = NormalizeRelativeAngle(predictedHeading + Limit(-maxTurning, moveAngle, maxTurning));
            
            // Adjust velocity based on direction
            predictedVelocity += (predictedVelocity * moveDir < 0 ? 2 * moveDir : moveDir);
            predictedVelocity = Limit(-8, predictedVelocity, 8);

            // Calculate the new predicted position
            predictedPosition = Project(predictedPosition, predictedHeading, predictedVelocity);

            counter++;

            if (Vector2.Distance(predictedPosition, wave.FireLocation) < 
                wave.DistanceTraveled + (counter * wave.BulletVelocity) + wave.BulletVelocity)
            {
                intercepted = true;
            }
        }

        return predictedPosition;
    }

    private double WallSmoothing(Vector2 botLocation, double angle, int direction)
    {
        // Keep checking points ahead along the angle until we find one that doesn't hit a wall
        while (!PointInBattlefield(Project(botLocation, angle, WALL_STICK)))
        {
            angle += direction * 0.05;
        }
        return angle;
    }

    private bool PointInBattlefield(Vector2 point)
    {
        return point.X > WALL_MARGIN && point.X < arenaWidth - WALL_MARGIN &&
               point.Y > WALL_MARGIN && point.Y < arenaHeight - WALL_MARGIN;
    }

    private int GetFactorIndex(EnemyWave wave, Vector2 targetLocation)
    {
        double offsetAngle = (AbsoluteBearing(wave.FireLocation, targetLocation) - wave.DirectAngle);
        double factor = NormalizeRelativeAngle(offsetAngle) / MaxEscapeAngle(wave.BulletVelocity) * wave.Direction;

        return (int)Limit(0, (factor * ((BINS - 1) / 2)) + ((BINS - 1) / 2), BINS - 1);
    }

    private void LogHit(EnemyWave wave, Vector2 targetLocation)
    {
        int index = GetFactorIndex(wave, targetLocation);

        for (int x = 0; x < BINS; x++)
        {
            // for the spot bin that we were hit on, add 1;
            // for the bins next to it, add 1/2;
            // for the next one, add 1/5, and so on...
            surfStats[x] += 1.0 / (Math.Pow(index - x, 2) + 1);
        }
    }

    private void AimAtTarget(double x, double y)
    {
        double bearing = BearingTo(x, y);
        turnDirection = (bearing >= 0) ? 1 : -1;
        SetTurnGunLeft(NormalizeRelativeAngle(GunDirection - DirectionTo(x, y)));
    }

    // Utility methods adapted for TankRoyale
    private Vector2 Project(Vector2 sourceLocation, double angle, double distance)
    {
        return new Vector2(
            (float)(sourceLocation.X + Math.Sin(angle) * distance),
            (float)(sourceLocation.Y + Math.Cos(angle) * distance));
    }

    private double AbsoluteBearing(Vector2 source, Vector2 target)
    {
        return Math.Atan2(target.X - source.X, target.Y - source.Y);
    }

    private double BulletVelocity(double power)
    {
        return 20 - 3 * power;
    }

    private double MaxEscapeAngle(double bulletVelocity)
    {
        return Math.Asin(8.0 / bulletVelocity);
    }

    private double Limit(double min, double value, double max)
    {
        return Math.Max(min, Math.Min(value, max));
    }

    private double NormalizeRelativeAngle(double angle)
    {
        double relativeAngle = angle % (2 * Math.PI);
        if (relativeAngle > Math.PI)
            relativeAngle -= 2 * Math.PI;
        else if (relativeAngle < -Math.PI)
            relativeAngle += 2 * Math.PI;
        return relativeAngle;
    }

    private double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }

    private double ToDegrees(double radians)
    {
        return radians * (180.0 / Math.PI);
    }

    private void SetBackAsFront(double goAngle)
    {
        double angle = NormalizeRelativeAngle(ToRadians(goAngle - Direction));
        if (Math.Abs(angle) > (Math.PI / 2))
        {
            if (angle < 0)
            {
                SetTurnRight(ToDegrees(Math.PI + angle));
            }
            else
            {
                SetTurnLeft(ToDegrees(Math.PI - angle));
            }
            SetBack(100);
        }
        else
        {
            if (angle < 0)
            {
                SetTurnLeft(ToDegrees(-1 * angle));
            }
            else
            {
                SetTurnRight(ToDegrees(angle));
            }
            SetAhead(100);
        }
    }
}

// Enemy Wave class
public class EnemyWave
{
    public long FireTime { get; set; }
    public double BulletVelocity { get; set; }
    public double DistanceTraveled { get; set; }
    public int Direction { get; set; }
    public double DirectAngle { get; set; }
    public Vector2 FireLocation { get; set; }
}