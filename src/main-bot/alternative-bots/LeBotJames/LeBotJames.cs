using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class LeBotJames : Bot
{   
    Random random = new Random();
    double xEnemy = 0;
    double yEnemy = 0;
    double distanceToEnemy = 0;
    double directionEnemy = 0;
    double speedEnemy = 0;
    double bulletSpeed;
    float shotsFired = 0;
    float shotsMissed = 0;
    int turnDirection = 1; 
    int collisionCounter = 0;
    bool stuck = false;

    static void Main(string[] args)
    {
        new LeBotJames().Start();
    }

    LeBotJames() : base(BotInfo.FromFile("LeBotJames.json")) { }
    
    public override void Run()
    {
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        stuck = false;

        TurretColor = Color.FromArgb(85, 37, 131);  
        RadarColor = Color.FromArgb(253, 185, 39);  
        BulletColor = Color.FromArgb(224, 122, 61);                
        ScanColor = Color.FromArgb(253, 185, 39);   
        TracksColor = Color.FromArgb(85, 37, 131);  
        GunColor = Color.FromArgb(85, 37, 131);      
        BodyColor = Color.FromArgb(253, 185, 39);    

        while (IsRunning)
        {   
            SetTurnRadarRight(360 * turnDirection);
            Go();
            if(EnemyCount > 1){
                stuck = false;
            }
            if((distanceToEnemy > 100 && EnemyCount> 1 && distanceToEnemy < 200) || stuck){
                //chill mode
                StrafeMove();
            }
            else{
                //aggressive mode
                SetForward(100);
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double angleToEnemy = BearingTo(xEnemy, yEnemy);
        if((EnemyCount > 1 && distanceToEnemy < 200) || stuck){
            //perpendicular to the enemy
            SetTurnLeft(angleToEnemy + 90); 
        }
        else{
            //targets the enemy
            SetTurnLeft(angleToEnemy); 
        }
        
        xEnemy = e.X;
        yEnemy = e.Y;
        distanceToEnemy = DistanceTo(xEnemy, yEnemy);
        directionEnemy = e.Direction;
        speedEnemy = e.Speed;
        
        
        LockRadarOnTarget(e.X, e.Y);
        SmartShot(xEnemy, yEnemy, speedEnemy, directionEnemy, distanceToEnemy);
        
        Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);
        Console.WriteLine("It's moving at " + speedEnemy + " at " + directionEnemy + " degrees");
    }

    public override void OnHitBot(HitBotEvent e)
    {
        collisionCounter++;
        if (collisionCounter > 4) 
        {
            Console.WriteLine("Stuck ramming an enemy! Escaping.");
            stuck = true;
            collisionCounter = 0;
        }    
    }


    private void LockRadarOnTarget(double x, double y)
    {      
        double angleToEnemy = Direction + BearingTo(x, y);
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);

        double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(x, y)), 45);

        radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);

        SetTurnRadarLeft(radarTurn);
    }


   public void SmartShot(double x, double y, double speed, double direction, double distance)
   {
        double bullet = CalcBullet(distance);
        bulletSpeed = CalcBulletSpeed(bullet);

        double enemyAngleRad = direction * Math.PI / 180;
        double enemyVx = speed * Math.Cos(enemyAngleRad);
        double enemyVy = speed * Math.Sin(enemyAngleRad);

        double dx = x - X;
        double dy = y - Y;

        double a = enemyVx * enemyVx + enemyVy * enemyVy - bulletSpeed * bulletSpeed;
        double b = 2 * (enemyVx * dx + enemyVy * dy);  
        double c = dx * dx + dy * dy;

        double d = b * b - 4 * a * c;
        if (d < 0) return; 

        double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        double t2 = (-b - Math.Sqrt(d)) / (2 * a);

        double timeToImpact = (t1 > 0 && t2 > 0) ? Math.Min(t1, t2) : Math.Max(t1, t2);
        if (timeToImpact < 0) return; 

        double enemyX = x + enemyVx * timeToImpact;
        double enemyY = y + enemyVy * timeToImpact;

        if (enemyX < 0) enemyX = 0;
        if (enemyY < 0) enemyY = 0;
        if (enemyX > 800) enemyX = 800;
        if (enemyY > 600) enemyY = 600;
        
        TurnGunToFaceTarget(enemyX, enemyY);

        Fire(bullet);
        shotsFired++;
        Console.WriteLine("Enemy velocity: " + enemyVx + ", " + enemyVy);
        Console.WriteLine("Predicted impact: " + enemyX + ", " + enemyY);
        Console.WriteLine("Time to impact: " + timeToImpact);
    }

    public double CalcBullet(double distance)
    {
        return 3 - (2.9 * distance / 800); 
    }

    private void TurnGunToFaceTarget(double x, double y)
    {
        var gunBearing = GunBearingTo(x, y);
        
        if (gunBearing >= 0)
            turnDirection = 1;
        else
            turnDirection = -1;
        SetTurnGunLeft(gunBearing);
        
    }

    public override void OnBulletHitWall(BulletHitWallEvent e)
    {
        shotsMissed++;
    }

    public override void OnRoundEnded(RoundEndedEvent e){
        float shotPersentage = 100 *(shotsFired - shotsMissed) / shotsFired;
        Console.WriteLine("I hit " + shotPersentage + " of my shots");
    }

    private void StrafeMove()
    {
        Forward(100 + 100 * random.NextDouble());
        Back(100 + 100 * random.NextDouble()); 
    }

    public override void OnWonRound(WonRoundEvent e)
    {
        TurnLeft(36_000);
    }
}