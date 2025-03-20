using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class B3040PKK : Bot
{   
    int turnDirection = 1;
    int lockedTargetId = -1;
    int lostTargetCount = 0;
    const int maxLostTargetCount = 3;
    double EnergiMusuh = 100;

    static void Main(string[] args)
    {
        new B3040PKK().Start();
    }

    B3040PKK() : base(BotInfo.FromFile("B3040PKK.json")) { }

    public override void Run()
    {
        BodyColor = Color.Red;
        GunColor = Color.Yellow;
        RadarColor = Color.Black;

        while (IsRunning)
        {
            if (Energy < 5) 
            {
                SetTurnRight(100);
                SetBack(120);
            }
            else if (RadarTurnRemaining == 0)
            {
                SetTurnRadarRight(Double.PositiveInfinity);
                JauhinTembok();
            }
            
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (Energy < 5) {
            SetForward(100);
            SetTurnRight(100);
        }
        else if  (lockedTargetId == -1 || lockedTargetId == e.ScannedBotId)
        {
            lockedTargetId = e.ScannedBotId;
            lostTargetCount = 0;

            double angleToEnemy = Direction + BearingTo(e.X, e.Y);
            double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
            double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(e.X, e.Y)), 45);

            radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);

            SetTurnRadarLeft(radarTurn);
            HadapTarget(e.X, e.Y);
            var distance = DistanceTo(e.X,e.Y);
            SetForward(distance+2);
            if (DistanceTo(e.X, e.Y) < 100) {
                Fire(3);
            }
            else if (DistanceTo(e.X, e.Y) < 300) {
                Fire(1.5);
            } 
            else {
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

    public override void OnHitBot(HitBotEvent e)
    {
        // Jika menabrak bot lain, langsung ubah target ke bot tersebut
        if (e.IsRammed) {
            lockedTargetId = e.VictimId;
            lostTargetCount = 0;
        }
        HadapTarget(e.X, e.Y);
        Fire(3);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
        JauhinTembok();

    }


    private void HadapTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        turnDirection = (bearing >= 0) ? 1 : -1;
        SetTurnLeft(bearing);
    }

    private void JauhinTembok()
    {
        double margin = 50;
        double arenaWidth = 800;
        double arenaHeight = 600;
        double x = X;
        double y = Y;

        if (x < margin && y > arenaHeight - margin) SetTurnRight(45);
        else if (x > arenaWidth - margin && y > arenaHeight - margin) SetTurnLeft(45);
        else if (x < margin && y < margin) SetTurnRight(135);
        else if (x > arenaWidth - margin && y < margin) SetTurnLeft(135);
        else if (x < margin) SetTurnRight(90);
        else if (x > arenaWidth - margin) SetTurnLeft(90);
        else if (y > arenaHeight - margin) SetTurnRight(180);
        else if (y < margin) SetTurnLeft(180);

    }
}