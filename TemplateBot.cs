using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TemplateBot : Bot
{   
    int turnDirection = 1;
    int lockedTargetId = -1;
    bool movingForward = true;
    int lostTargetCount = 0;
    const int maxLostTargetCount = 20;

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

            radarTurn = (radarTurn + (radarTurn < 0 ? -extraTurn : extraTurn));

            SetTurnRadarLeft(radarTurn);
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
        TurnLeft(bearing);
    }

    private void ReverseDirection()
    {
        movingForward = !movingForward;
        if (movingForward)
        {
            Forward(100);
        }
        else
        {
            Back(100);
        }
    }
}
