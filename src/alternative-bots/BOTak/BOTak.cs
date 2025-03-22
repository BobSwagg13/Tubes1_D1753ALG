using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BOTak : Bot
{
    // Simple tracking of the closest enemy
    private double enemyX, enemyY, enemyDistance;
    private double enemyEnergy = 100;
    private double lastEnemyEnergy = 100;
    
    // Basic movement control
    private Random random = new Random();
    private int moveDirection = 1;
    private int wallHitCount = 0;
    
    // Constants for survivability
    private const double WALL_MARGIN = 100;
    private const double DANGER_DISTANCE = 200;
    
    static void Main(string[] args)
    {
        new BOTak().Start();
    }
    
    BOTak() : base(BotInfo.FromFile("BOTak.json")) { }
    
    public override void Run()
    {
        // Setup for optimal tracking
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        
        // Set colors
        BodyColor = Color.DarkGray;
        GunColor = Color.Black;
        RadarColor = Color.Red;
        
        while (IsRunning)
        {
            // Always keep radar spinning for situational awareness
            SetTurnRadarRight(Double.PositiveInfinity);
            
            // Core greedy survivability strategy
            SurvivabilityMovement();
            
            Go();
        }
    }
    
    private void SurvivabilityMovement()
    {
        // No enemy detected - stay near walls for cover
        if (enemyDistance == 0)
        {
            StayNearWalls();
            return;
        }
        
        // Check if enemy fired (energy drop between 0.1 and 3.0)
        if (lastEnemyEnergy > enemyEnergy && (lastEnemyEnergy - enemyEnergy) <= 3.0 && (lastEnemyEnergy - enemyEnergy) >= 0.1)
        {
            // Evasive maneuver - perpendicular movement is best against bullets
            EvadeBullet();
        }
        else if (enemyDistance < DANGER_DISTANCE)
        {
            // Too close - create distance immediately
            RunAway();
        }
        else
        {
            // Safe distance - move perpendicular to enemy for harder targeting
            OrbitEnemy();
        }
        
        // Update last energy for bullet detection
        lastEnemyEnergy = enemyEnergy;
    }
    
    private void StayNearWalls()
    {
        // Find if we're near any wall
        bool nearWall = (X < WALL_MARGIN || Y < WALL_MARGIN || 
                         X > ArenaWidth - WALL_MARGIN || Y > ArenaHeight - WALL_MARGIN);
        
        if (nearWall)
        {
            // Follow wall perimeter
            SetTurnRight(45 * moveDirection);
            SetForward(100);
            
            // Random direction change for unpredictability
            if (random.NextDouble() < 0.1)
                moveDirection *= -1;
        }
        else
        {
            // Move toward nearest wall
            double distToLeft = X;
            double distToRight = ArenaWidth - X;
            double distToBottom = Y;
            double distToTop = ArenaHeight - Y;
            
            // Find closest wall and move toward it
            if (distToLeft <= distToRight && distToLeft <= distToBottom && distToLeft <= distToTop)
                SetTurnLeft(NormalizeAngle(270 - Direction));
            else if (distToRight <= distToLeft && distToRight <= distToBottom && distToRight <= distToTop)
                SetTurnLeft(NormalizeAngle(90 - Direction));
            else if (distToBottom <= distToLeft && distToBottom <= distToRight && distToBottom <= distToTop)
                SetTurnLeft(NormalizeAngle(180 - Direction));
            else
                SetTurnLeft(NormalizeAngle(0 - Direction));
                
            SetForward(100);
        }
    }
    
    private void EvadeBullet()
    {
        // Enhanced perpendicular movement to avoid bullet
        double angleToEnemy = BearingTo(enemyX, enemyY);
        
        // Randomly select evasion direction
        int evasionDirection = (random.NextDouble() > 0.5) ? 1 : -1;
        
        // Turn perpendicular to enemy
        SetTurnLeft(angleToEnemy + 90 * evasionDirection);
        
        // Move distance based on how close enemy is
        double evasionDistance = Math.Min(150, 300000 / (enemyDistance * enemyDistance + 1));
        SetForward(evasionDistance);
    }
    
    private void RunAway()
    {
        // Move directly away from enemy
        double angleToEnemy = BearingTo(enemyX, enemyY);
        SetTurnLeft(angleToEnemy + 180);
        SetForward(150);
    }
    
    private void OrbitEnemy()
    {
        // Circle enemy at safe distance
        double angleToEnemy = BearingTo(enemyX, enemyY);
        SetTurnLeft(angleToEnemy + 90 * moveDirection);
        SetForward(80);
        
        // Occasionally change direction for unpredictability
        if (random.NextDouble() < 0.05)
            moveDirection *= -1;
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Update enemy information
        enemyX = e.X;
        enemyY = e.Y;
        enemyDistance = DistanceTo(e.X, e.Y);
        enemyEnergy = e.Energy;
        
        // Lock radar on enemy with oscillation for better tracking
        double angleToEnemy = Direction + BearingTo(e.X, e.Y);
        double radarTurn = NormalizeAngle(angleToEnemy - RadarDirection);
        radarTurn += (radarTurn < 0 ? -15 : 15); // Small oscillation
        SetTurnRadarLeft(radarTurn);
        
        // Simple direct fire - no prediction
        FireDirectly(e);
    }
    
    private void FireDirectly(ScannedBotEvent e)
    {
        // Save energy for movement if getting low
        if (Energy < 20) return;
        
        // Calculate distance-based power
        // Close = high power, Far = low power
        double bulletPower;
        
        if (enemyDistance < 100) {
            // Very close - maximum power
            bulletPower = 3.0;
        } else if (enemyDistance < 200) {
            // Medium range - medium power
            bulletPower = 2.0;
        } else if (enemyDistance < 400) {
            // Long range - lower power
            bulletPower = 1.0;
        } else {
            // Very far - minimum power
            bulletPower = 0.1;
        }
        
        // Aim directly at the enemy's current position
        double gunBearing = GunBearingTo(enemyX, enemyY);
        SetTurnGunLeft(gunBearing);
        
        // Fire when gun is on target
        if (Math.Abs(gunBearing) < 5) {
            Fire(bulletPower);
        }
    }
    
    private double NormalizeAngle(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
    
    public override void OnHitBot(HitBotEvent e)
    {
        // Back away immediately
        SetBack(100);
        SetTurnRight(90);
        Go();
    }
    
    public override void OnHitWall(HitWallEvent e)
    {
        // Get away from wall
        SetBack(50);
        SetTurnRight(90);
        
        // If hitting walls repeatedly, move toward center
        wallHitCount++;
        if (wallHitCount > 2)
        {
            double angleToCenter = BearingTo(ArenaWidth/2, ArenaHeight/2);
            SetTurnLeft(angleToCenter);
            SetForward(200);
            wallHitCount = 0;
        }
    }
}