using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;
using System.Linq;



public class SlayF3812FIO : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    Point2D enemyPosition = new Point2D(300, 400);
    double moveLength = 0;
    double enemyDistance = 0; // Menyimpan jarak ke musuh
    double idealDistance = 200;
    bool targetLocked = false;
    int lockedTargetId = -1;
    int lostTargetCount = 0;
    int moveIncrement = 1;
    int maxLostTargetCount = 3;
    int shotsMissed = 0;
    int shotsHit = 0;
    double d = 250;
    double directionEnemy = 0;
    double speedEnemy = 0;
    double bulletSpeed;
    int collisionCounter = 0;
    bool stuck = false;
    Random rand = new Random();

    static void Main(string[] args)
    {
        new SlayF3812FIO
        ().Start();
    }

    SlayF3812FIO() : base(BotInfo.FromFile("SlayF3812FIO.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;
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
        while (IsRunning)
        {
            if (!targetLocked)
            {
                SetTurnRadarRight(Double.PositiveInfinity);
                Console.WriteLine("distance: " + enemyDistance);
                Console.WriteLine("d: " + d);
            }
                // SetTurnRadarRight(360);
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

        if (EnemyCount > 2){
            d = 400;
        }else{
            d = 250;
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


            // if (enemyDistance >= 220 && enemyDistance <= 260){
            //     SetForward(0); SetBack(0); Go();
            // }

            // if (enemyDistance < 220  || enemyDistance > 260)
            // {
            //     KeepDistance();
            // }

            KeepDistance();
            Go();
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

    public void gunToEnemy(double x, double y)
    {
        double gunTurn = NormalizeRelativeAngle(GunBearingTo(x, y));
        SetTurnGunLeft(gunTurn);
        while (GunTurnRemaining > 0)
        {
            Go();
        }

    }

    public override void OnBulletHitWall(BulletHitWallEvent e)
    {
        shotsMissed++;
        if (shotsMissed > 3)
        {
            d -= 30;
            d = Math.Clamp(d, 40, 250);
            shotsHit = 0;
        }
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        shotsHit++;
        if (shotsHit > 3 && e.VictimId == lockedTargetId)
        {
            d = 250;
            shotsMissed = 0;
        }
    }




    public void runAway()
    {
        //buat 8 kasus arah
        if (RadarDirection > 0 && RadarDirection <= 45){GoToPosition(300, 40);}
        else if (RadarDirection > 45 && RadarDirection <= 90){GoToPosition(40, 250);}
        else if (RadarDirection > 90 && RadarDirection <= 135){GoToPosition(760, 250);}
        else if (RadarDirection > 135 && RadarDirection <= 180){GoToPosition(500, 40);}
        else if (RadarDirection > 180 && RadarDirection <= 225){GoToPosition(500, 560);}
        else if (RadarDirection > 225 && RadarDirection <= 270){GoToPosition(760, 350);}
        else if (RadarDirection > 270 && RadarDirection <= 315){GoToPosition(40, 350);}
        else if (RadarDirection > 315 && RadarDirection <= 360){GoToPosition(300, 560);}
    }

    public void KeepDistance()
    {
        if (stuck){
            return;
        }
        double angle1 = ((RadarDirection - 135) + 360)%360 * Math.PI / 180;
        double angle2 = ((RadarDirection + 135) + 360)%360 * Math.PI / 180;


        Point2D point1 = enemyPosition + new Point2D(d * Math.Cos(angle1), d * Math.Sin(angle1));
        Point2D point2 = enemyPosition + new Point2D(d * Math.Cos(angle2), d * Math.Sin(angle2));


        //cari point yang tidak deket ke tembok
        if (!(point1.X < 50 || point1.X > 750 || point1.Y < 50 || point1.Y > 550) && enemyDistance >200)
        {
            GoToPosition(point1.X, point1.Y);
        }
        else if (!(point2.X < 50 || point2.X > 750 || point2.Y < 50 || point2.Y > 550))
        {
            GoToPosition(point2.X, point2.Y);
        }else{
            runAway();
        }
        
        Console.WriteLine("enemyPosition : " + enemyPosition.X + " " + enemyPosition.Y);
        Console.WriteLine("point1 : " + point1.X + " " + point1.Y + "   Angel: " + angle1);
        Console.WriteLine("point2 : " + point2.X + " " + point2.Y + "   Angel: " + angle2);
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
        Console.WriteLine("From: " + X + " " + Y);
        Console.WriteLine("To: " + x + " " + y);
    }

    public void GoToEnemy()
    {
        GoToPosition(enemyPosition.X, enemyPosition.Y);
    }

    public override void OnTick(TickEvent e)
    {
        if(e.TurnNumber % 20 == 0){
            stuck = false;
        }
    }


    public override void OnHitBot(HitBotEvent e)
    {
        lockedTargetId = e.VictimId;
        enemyPosition = new Point2D(e.X, e.Y);
        gunToEnemy(e.X, e.Y);
        Fire(3);

        collisionCounter++;
        if (collisionCounter > 3) 
        {
            Console.WriteLine("Stuck ramming an enemy! Escaping.");
            stuck = true;
            unstuck();
            collisionCounter = 0;
        }
    }

    //unstuck (belakangin musuh, terus kabur kearah kiri/kanan berdasar posisi (mirip run away))
    public void unstuck()
    {
        if (RadarDirection - directionEnemy > 0){
            TurnRight(80);
            SetTurnLeft(10); SetForward(50); Go();
        }else{
            TurnLeft(80);
            SetTurnRight(10); SetForward(50); Go();
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {

    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        
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

}
