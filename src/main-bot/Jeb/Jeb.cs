using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;



public class Jeb : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    Point2D enemyPosition = new Point2D(300, 400);
    double moveLength = 0;
    double enemyDistance = 0; // Menyimpan jarak ke musuh
    bool targetLocked = false;
    int lockedTargetId = -1;
    int lostTargetCount = 0;
    int moveIncrement = 1;
    int maxLostTargetCount = 3;
    int shotsMissed = 0;
    int shotsHit = 0;
    double d = 250;
    double extraD = 0;
    int loopCount = 0;
    double bulletSpeed;
    int collisionCounter = 0;
    bool stuck = false;
    double hue = 0;
    bool getRammed = false;
    int turnRammed = 1;
    int turnStuck = 1;
    Random rand = new Random();
    // Color t = HsvToRgb(hue, 1.0, 1.0);  

    static void Main(string[] args)
    {
        new Jeb
        ().Start();
    }

    Jeb() : base(BotInfo.FromFile("Jeb.json")) { }

    public override void Run()
    {
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        TurretColor = Color.FromArgb(85, 37, 255);  
        RadarColor = Color.FromArgb(253, 185, 255);  
        BulletColor = Color.FromArgb(224, 122, 255);                
        ScanColor = Color.FromArgb(253, 185, 255);   
        TracksColor = Color.FromArgb(85, 37, 255);  
        GunColor = Color.FromArgb(85, 37, 255);      
        BodyColor = Color.FromArgb(253, 185, 255);    
        MaxRadarTurnRate = 45;
        turnStuck = 1;
        turnRammed = 1;
        while (IsRunning)
        {
            loopCount++;
            // if (loopCount%14 > 7){
            //     RadarColor = Color.FromArgb(255, 0, 0);  
            //     ScanColor = Color.FromArgb(255, 0, 0);  
            // }else{
            //     RadarColor = Color.FromArgb(0, 0, 255);  
            //     ScanColor = Color.FromArgb(0, 0, 255);  
            // }

            if (!targetLocked)
            {
                SetTurnRadarRight(Double.PositiveInfinity);
            }
            Go();
        }
    }

    public void letsGoJeb(){
        hue = (hue + 2) % 360;
        // TurretColor = HsvToRgb(hue, 0.93, 0.8);  
        // RadarColor = HsvToRgb(hue, 1.0, 1.0); 
        // BulletColor = HsvToRgb(hue, 1.0, 1.0);             
        // ScanColor = HsvToRgb(hue, 1.0, 1.0); 
        // TracksColor = HsvToRgb(hue, 0.93, 0.68);
        // GunColor = HsvToRgb(hue, 1.0, 1.0);    
        // BodyColor = HsvToRgb(hue, 1.0, 1.0);
        TurretColor = HsvToRgb(hue, 0.62, 0.8);  
        RadarColor = HsvToRgb(hue, 0.62, 1.0); 
        BulletColor = HsvToRgb(hue, 0.62, 1.0);             
        ScanColor = HsvToRgb(hue, 1.0, 1.0); 
        TracksColor = HsvToRgb(hue, 0.62, 0.7);
        GunColor = HsvToRgb(hue, 0.62, 1.0);    
        BodyColor = HsvToRgb(hue, 0.62, 1.0);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

        if (e.Energy == 0){
            //fire directly
            SmartShot(e.X, e.Y, 0, 0, DistanceTo(e.X, e.Y));
        }

        if (lockedTargetId == -1 || lockedTargetId == e.ScannedBotId)
        {
            lockedTargetId = e.ScannedBotId;
            lostTargetCount = 0;

            double radarTurn = NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
            double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(e.X, e.Y)), 45);
            radarTurn = radarTurn + (radarTurn < 0 ? -extraTurn : extraTurn);
            SetTurnRadarLeft(radarTurn);

            enemyPosition = new Point2D(e.X, e.Y);
            enemyDistance = DistanceTo(e.X, e.Y);
            SmartShot(e.X, e.Y, e.Speed, e.Direction, enemyDistance);

            KeepDistance();
            Go();
        }
        else
        {
            ScanColor = Color.FromArgb(255, 247, 168);
            lostTargetCount++;
            if (lostTargetCount >= maxLostTargetCount)
            {
                lockedTargetId = -1;
                SetTurnRadarRight(Double.PositiveInfinity);
            }
        }
    }

    public override void OnBulletHitWall(BulletHitWallEvent e)
    {
        shotsMissed++;
        Console.WriteLine("Miss " + shotsMissed);
        if (shotsMissed >= 2)
        {
            Console.WriteLine("deketin lagi");
            extraD -= 30;
            shotsHit = 0;
        }
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        if (e.VictimId == lockedTargetId){
            shotsHit++;
        }
        if (shotsHit > 2)
        {
            Console.WriteLine("Jaga jarak lagi");
            extraD += 30;
            shotsMissed = 0;
            shotsHit = 0;
        }
    }




    public void runAway()
    {
        //buat 8 kasus arah
        Console.WriteLine("Run away");
        // ScanColor = Color.FromArgb(255, 189, 74);

        if (RadarDirection > 0 && RadarDirection <= 45){GoToPosition(500, 40);}
        else if (RadarDirection > 45 && RadarDirection <= 90){GoToPosition(40, 350);}
        else if (RadarDirection > 90 && RadarDirection <= 135){GoToPosition(300, 40);}
        else if (RadarDirection > 135 && RadarDirection <= 180){GoToPosition(760, 350);}
        else if (RadarDirection > 180 && RadarDirection <= 225){GoToPosition(760, 250);}
        else if (RadarDirection > 225 && RadarDirection <= 270){GoToPosition(300, 560);}
        else if (RadarDirection > 270 && RadarDirection <= 315){GoToPosition(500, 560);}
        else if (RadarDirection > 315 && RadarDirection <= 360){GoToPosition(40, 250);}
    }

    public void KeepDistance()
    {
        double angle1 = ((RadarDirection - 135) + 360)%360 * Math.PI / 180;
        double angle2 = ((RadarDirection + 135) + 360)%360 * Math.PI / 180;
        double angle3 = ((RadarDirection + 120) + 360)%360 * Math.PI / 180;
        double angle4 = ((RadarDirection - 120) + 360)%360 * Math.PI / 180;


        Point2D point1 = enemyPosition + new Point2D(d * Math.Cos(angle1), d * Math.Sin(angle1));
        Point2D point2 = enemyPosition + new Point2D(d * Math.Cos(angle2), d * Math.Sin(angle2));
        Point2D point3 = enemyPosition + new Point2D((EnemyCount > 2 ? 250 : 150) * Math.Cos(angle4), (EnemyCount > 2 ? 250 : 150) * Math.Sin(angle4));
        Point2D point4 = enemyPosition + new Point2D((EnemyCount > 2 ? 250 : 150) * Math.Cos(angle3), (EnemyCount > 2 ? 250 : 150) * Math.Sin(angle3));


        //cari point yang tidak deket ke tembok
        if (!(point1.X < 50 || point1.X > 750 || point1.Y < 50 || point1.Y > 550) && enemyDistance >200)
        {
            Console.WriteLine("Point   1");
            GoToPosition(point1.X, point1.Y);
        }
        else if (!(point2.X < 50 || point2.X > 750 || point2.Y < 50 || point2.Y > 550))
        {
            Console.WriteLine("Point   2");
            GoToPosition(point2.X, point2.Y);
        }
        else if (!(point3.X < 50 || point3.X > 750 || point3.Y < 50 || point3.Y > 550) && enemyDistance > 40)
        {
            Console.WriteLine("Point   3");
            GoToPosition(point3.X, point3.Y);
        }
        else if (!(point4.X < 50 || point4.X > 750 || point4.Y < 50 || point4.Y > 550))
        {
            Console.WriteLine("Point   4");
            GoToPosition(point4.X, point4.Y);
        }
        else
        {
            runAway();
        }
    }

    public void GoToPosition(double x, double y)
    {
        double bodyTurn = NormalizeRelativeAngle(BearingTo(x, y));
        double distance = DistanceTo(x, y);
        // double gunTurn = NormalizeRelativeAngle(GunBearingTo(x, y));
        // SetTurnGunLeft(gunTurn);

        if (Math.Abs(bodyTurn) > 90){
            if (bodyTurn < 0) {bodyTurn = -180-bodyTurn;}
            else {bodyTurn = 180-bodyTurn;}
            SetTurnRight(bodyTurn);
            SetBack(distance);
        }else{
            SetTurnLeft(bodyTurn);
            SetForward(distance);
        }
    }

    public void GoToEnemy()
    {
        GoToPosition(enemyPosition.X, enemyPosition.Y);
    }

    public override void OnTick(TickEvent e)
    {
        Console.WriteLine("d : " + d);
        Console.WriteLine(turnRammed + " " + turnStuck + " " + getRammed + " " + stuck +"         "+e.TurnNumber);
        letsGoJeb();
        if (EnemyCount > 2){
            extraD = Math.Clamp(extraD, -300, 0);
            d = 450 + extraD;
        }else{
            extraD = Math.Clamp(extraD, -100, 0);
            d = 250 + extraD;
        }
        if (!getRammed){
            turnRammed++;
        }
        if (!stuck){
            turnStuck++;
        }
        if (lockedTargetId == -1){
            extraD += 50;
        }
        if(e.TurnNumber - turnStuck > 20 && stuck){
            stuck = false;
            SetForward(0);
            Console.WriteLine("aman dari stuck");
        }
        if(e.TurnNumber - turnRammed > 30){
            getRammed = false;
            turnRammed = e.TurnNumber + turnRammed;
        }
    }


    public override void OnHitBot(HitBotEvent e)
    {
        if (!stuck){ //hanya refelek ketika pertama nabrak
            if (DistanceRemaining < -10){SetForward(60);Go();}
            if (DistanceRemaining > 10){SetBack(60);Go();}
        }
        if (!getRammed){ //jika ditabrak 2 bot, lock 1 musuh untuk beberapa turn dulu
            lockedTargetId = e.VictimId;
            enemyPosition = new Point2D(e.X, e.Y);
            Console.WriteLine("Ketabrak ama " + e.VictimId);
            getRammed = true;
        }

        collisionCounter++;
        if (collisionCounter > 3) 
        {
            lockedTargetId = e.VictimId;
            enemyPosition = new Point2D(e.X, e.Y);
            double radarTurn = NormalizeRelativeAngle(RadarBearingTo(enemyPosition.X, enemyPosition.Y));
            double gunTurn = NormalizeRelativeAngle(GunBearingTo(enemyPosition.X, enemyPosition.Y));
            double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(enemyPosition.X, enemyPosition.Y)), 45);
            radarTurn = radarTurn + (radarTurn < 0 ? -extraTurn : extraTurn);
            SetTurnRadarLeft(0); SetTurnRadarLeft(radarTurn);SetTurnGunLeft(gunTurn);
            Go();
            Fire(3);
            Console.WriteLine("Stuck ramming an enemy! Escaping.");
            stuck = true;
            unstuck();
            collisionCounter = 0;
        }
    }

    //unstuck (belakangin musuh, terus kabur kearah kiri/kanan berdasar posisi (mirip run away))
    public void unstuck()
    {
        Console.WriteLine("Enemy make me stuck in " + RadarDirection);
        if ((RadarDirection + 180) <= Direction || Direction >= RadarDirection-180){
            TurnRight(180 - (RadarDirection - Direction + 360)%360);
        }else{
            TurnLeft(180 - (Direction - RadarDirection + 360)%360);
        }
        SetForward(100); Go(); KeepDistance();
        // bagi 8 kasus cari jarak terdekat ke tembok sebelah mana?
        // misal 40, 70 oh ini lebih deket ke tembok kiri dibanding tembok bawah
        // kabur ke arah bawah
        
    }

    public override void OnDeath(DeathEvent e)
    {
        stuck = false;
        getRammed = false;
    }

    public override void OnHitWall(HitWallEvent e)
    {

    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        extraD += 80;
    }
    public void SmartShot(double x, double y, double speed, double direction, double distance)
    {
        double bullet = CalcBullet(distance);

        if (stuck){bullet = 3;}
        if (Energy < 4){bullet = 0.5;}
        if (Energy < 2){bullet = 0.2;}

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
    }

    public double CalcBullet(double distance)
    {
        return 3 - (2.9 * distance / 800); 
    }

    private void TurnGunToFaceTarget(double x, double y)
    {
        var gunBearing = GunBearingTo(x, y);
        SetTurnGunLeft(gunBearing);
    }

    private Color HsvToRgb(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r = 0, g = 0, b = 0;

        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return Color.FromArgb(
            (int)((r + m) * 255),
            (int)((g + m) * 255),
            (int)((b + m) * 255)
        );
    }
}
