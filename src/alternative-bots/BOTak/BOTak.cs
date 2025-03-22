using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BOTak : Bot
{
    private double enemyX, enemyY, enemyDistance;
    private double enemyEnergy = 100;
    private double lastEnemyEnergy = 100;
    
    private Random random = new Random();
    private int moveDirection = 1;
    private int wallHitCount = 0;
    
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
            SetTurnRadarRight(Double.PositiveInfinity);
            
            SurvivabilityMovement();
            
            Go();
        }
    }
    
    private void SurvivabilityMovement()
    {
        if (enemyDistance == 0)
        {
            StayNearWalls();
            return;
        }
        
        if (lastEnemyEnergy > enemyEnergy && (lastEnemyEnergy - enemyEnergy) <= 3.0 && (lastEnemyEnergy - enemyEnergy) >= 0.1)
        {
            EvadeBullet();
        }
        else if (enemyDistance < DANGER_DISTANCE)
        {
            RunAway();
        }
        else
        {
            OrbitEnemy();
        }
        
        lastEnemyEnergy = enemyEnergy;
    }
    
    private void StayNearWalls()
    {
        bool nearWall = (X < WALL_MARGIN || Y < WALL_MARGIN || 
                         X > ArenaWidth - WALL_MARGIN || Y > ArenaHeight - WALL_MARGIN);
        
        if (nearWall)
        {
            SetTurnRight(45 * moveDirection);
            SetForward(100);
            
            if (random.NextDouble() < 0.1)
                moveDirection *= -1;
        }
        else
        {
            double distToLeft = X;
            double distToRight = ArenaWidth - X;
            double distToBottom = Y;
            double distToTop = ArenaHeight - Y;
            
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
        double angleToEnemy = BearingTo(enemyX, enemyY);
        
        int evasionDirection = (random.NextDouble() > 0.5) ? 1 : -1;
        
        SetTurnLeft(angleToEnemy + 90 * evasionDirection);
        
        double evasionDistance = Math.Min(150, 300000 / (enemyDistance * enemyDistance + 1));
        SetForward(evasionDistance);
    }
    
    private void RunAway()
    {
        double angleToEnemy = BearingTo(enemyX, enemyY);
        SetTurnLeft(angleToEnemy + 180);
        SetForward(150);
    }
    
    private void OrbitEnemy()
    {
        double angleToEnemy = BearingTo(enemyX, enemyY);
        SetTurnLeft(angleToEnemy + 90 * moveDirection);
        SetForward(80);
        
        if (random.NextDouble() < 0.05)
            moveDirection *= -1;
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
    {
        enemyX = e.X;
        enemyY = e.Y;
        enemyDistance = DistanceTo(e.X, e.Y);
        enemyEnergy = e.Energy;
        
        double angleToEnemy = Direction + BearingTo(e.X, e.Y);
        double radarTurn = NormalizeAngle(angleToEnemy - RadarDirection);
        radarTurn += (radarTurn < 0 ? -15 : 15);
        SetTurnRadarLeft(radarTurn);
        
        FireDirectly(e);
    }
    
    private void FireDirectly(ScannedBotEvent e)
    {
        if (Energy < 20) return;
        
        double bulletPower;
        
        if (enemyDistance < 100) {
            bulletPower = 3.0;
        } else if (enemyDistance < 200) {
            bulletPower = 2.0;
        } else if (enemyDistance < 400) {
            bulletPower = 1.0;
        } else {
            bulletPower = 0.5;
        }
        
        double gunBearing = GunBearingTo(enemyX, enemyY);
        SetTurnGunLeft(gunBearing);
        
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
        SetBack(100);
        SetTurnRight(90);
        Go();
    }
    
    public override void OnHitWall(HitWallEvent e)
    {
        SetBack(50);
        SetTurnRight(90);
        
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