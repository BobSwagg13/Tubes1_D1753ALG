using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TemplateBot : Bot
{   
    int turnDirection = 1;
    int lockedTargetId = -1;
    int lostTargetCount = 0;
    const int maxLostTargetCount = 5;
    double EnergiMusuh = 100;

    static void Main(string[] args)
    {
        new TemplateBot().Start();
    }

    TemplateBot() : base(BotInfo.FromFile("TemplateBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;
        GunColor = Color.Black;
        RadarColor = Color.Red;

        while (IsRunning)
        {
            if (RadarTurnRemaining == 0)
            {
                SetTurnRadarRight(Double.PositiveInfinity);
                JauhinTembok();
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (lockedTargetId == -1 || lockedTargetId == e.ScannedBotId)
        {
            lockedTargetId = e.ScannedBotId;
            lostTargetCount = 0;

            double angleToEnemy = Direction + BearingTo(e.X, e.Y);
            double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);

            double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(e.X, e.Y)), 45);

            radarTurn = radarTurn + (radarTurn < 0 ? -extraTurn : extraTurn);

            SetTurnRadarLeft(radarTurn);
            HadapTarget(e.X,e.Y);

            double energyDrop = EnergiMusuh - e.Energy;
            if (energyDrop >= 0.1 && energyDrop <= 3)
            {
            
            }

            EnergiMusuh = e.Energy;
            
            if (Energy - e.Energy >= 10) {
                var distance = DirectionTo(e.X,e.Y);
                SetForward(distance+3);
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
        HadapTarget(e.X, e.Y);
        Fire(3);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
        Rescan();
    }

    private void HadapTarget(double x, double y)
    {

        var bearing = BearingTo(x, y);
        turnDirection = (bearing >= 0) ? 1 : -1;
        SetTurnLeft(bearing);
    }

    private void JauhinTembok()
{
    double margin = 50; // Batas aman dari tembok
    double arenaWidth = 800;  // Lebar arena (ubah sesuai ukuran)
    double arenaHeight = 600; // Tinggi arena (ubah sesuai ukuran)

    // Ambil posisi bot
    double x = X;
    double y = Y;

    // Sudut Kiri Atas
    if (x < margin && y > arenaHeight - margin)
    {
        SetTurnRight(45); // Arahkan ke kanan bawah
        SetForward(100);
    }
    // Sudut Kanan Atas
    else if (x > arenaWidth - margin && y > arenaHeight - margin)
    {
        SetTurnLeft(45); // Arahkan ke kiri bawah
        SetForward(30);
    }
    // Sudut Kiri Bawah
    else if (x < margin && y < margin)
    {
        SetTurnRight(135); // Arahkan ke kanan atas
        SetForward(30);
    }
    // Sudut Kanan Bawah
    else if (x > arenaWidth - margin && y < margin)
    {
        SetTurnLeft(135); // Arahkan ke kiri atas
        SetForward(30);
    }
    // Dekat Tembok Kiri
    else if (x < margin)
    {
        SetTurnRight(90); // Arahkan ke kanan
        SetForward(30);
    }
    // Dekat Tembok Kanan
    else if (x > arenaWidth - margin)
    {
        SetTurnLeft(90); // Arahkan ke kiri
        SetForward(30);
    }
    // Dekat Tembok Atas
    else if (y > arenaHeight - margin)
    {
        SetTurnRight(180); // Arahkan ke bawah
        SetForward(30);
    }
    // Dekat Tembok Bawah
    else if (y < margin)
    {
        SetTurnLeft(180); // Arahkan ke atas
        SetForward(30);
    }
}


}
