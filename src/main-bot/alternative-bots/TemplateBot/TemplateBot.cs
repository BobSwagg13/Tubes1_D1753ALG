using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TemplateBot : Bot
{   

    double xEnemy = 0;
    double yEnemy = 0;
    double distanceToEnemy = 0;
    double directionEnemy = 0;
    double speedEnemy = 0;
    double bulletSpeed;
    float shotsFired = 0;
    float shotsMissed = 0;
    int turnDirection = 1; 
    
    static void Main(string[] args)
    {
        new TemplateBot().Start();
    }

    TemplateBot() : base(BotInfo.FromFile("TemplateBot.json")) { }
    
    public override void Run()
    {
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.Gray;
        while (IsRunning)
        {
            //trafeMove();
            if(distanceToEnemy > 100 && EnemyCount> 1){
                StrafeMove();
                
            }
            else{
                Forward(100);
            }
            
            TurnRadarRight(360 * turnDirection);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double angleToEnemy = BearingTo(xEnemy, yEnemy);
        SetTurnLeft(angleToEnemy + 90);
        
        xEnemy = e.X;
        yEnemy = e.Y;
        distanceToEnemy = DistanceTo(xEnemy, yEnemy);
        directionEnemy = e.Direction;
        speedEnemy = e.Speed;
        
        // Keep radar locked on target
        LockRadarOnTarget(e.X, e.Y);

        // Aim gun separately
        

        SmartShot(xEnemy, yEnemy, speedEnemy, directionEnemy, distanceToEnemy);
        
        Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);
        Console.WriteLine("It's moving at " + speedEnemy + " at " + directionEnemy + " degrees");
    }


    private void LockRadarOnTarget(double x, double y)
    {      
        double angleToEnemy = Direction + BearingTo(x, y);
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);

        // Add extra turn to ensure we keep scanning even if enemy dodges
        double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(x, y)), 45);

        radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);

        SetTurnRadarLeft(radarTurn);
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        SmartShot(xEnemy, yEnemy, speedEnemy, directionEnemy, distanceToEnemy);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

   public void SmartShot(double x, double y, double speed, double direction, double distance) {
        double bullet = 3 - (2.9 * distance / 800);
        bullet = distance < 100 ? 3 : distance < 300 ? 2 : 1;
        
        
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

    
    public void SmartFire(double distance){
        shotsFired++;
        double firePower = 1;
        bulletSpeed = CalcBulletSpeed(firePower);

        Fire(firePower);
        
        
    }

    private void TurnToFaceTarget(double x, double y)
    {        
        var bearing = BearingTo(x, y);
        if (bearing >= 0)
            turnDirection = 1;
        else
            turnDirection = -1;

        SetTurnLeft(bearing);
        // Go();
    }

    private void TurnGunToFaceTarget(double x, double y)
    {
        var gunBearing = GunBearingTo(x, y);
        
        if (gunBearing >= 0)
            turnDirection = 1;
        else
            turnDirection = -1;
        //TurnGunLeft(gunBearing);
        SetTurnGunLeft(gunBearing);
        
    }

    private void TurnRadarToFaceTarget(double x, double y){
        var gunBearing = GunBearingTo(x, y);
        
        if (gunBearing >= 0)
            turnDirection = 1;
        else
            turnDirection = -1;
        //TurnGunLeft(gunBearing);
        SetTurnRadarLeft(gunBearing);
    }

    public override void OnBulletHitWall(BulletHitWallEvent e){
        shotsMissed++;
    }

    public override void OnRoundEnded(RoundEndedEvent e){
        float shotPersentage = 100 *(shotsFired - shotsMissed) / shotsFired;
        Console.WriteLine("I hit " + shotPersentage + " of my shots");
    }

    private void StrafeMove()
{
    double turnAngle = 45 * Math.Sin(Direction * Math.PI / 180);  // Change heading smoothly
    SetTurnLeft(turnAngle);
    SetForward(100);  
}



}
    // Method to handle gun targeting with prediction
    


