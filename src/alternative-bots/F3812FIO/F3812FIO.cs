using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;
using System.Linq;



public class F3812FIO : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    Point2D enemyPosition = new Point2D(300, 400);
    double moveLength = 0;
    private double enemyDistance; // Menyimpan jarak ke musuh
    double idealDistance = 200;
    bool targetLocked = false;
    int lockedTargetId = -1;
    int lostTargetCount = 0;
    int patternLength = 20;
    int maxLostTargetCount = 3;
    int moveIncrement = 1;
    Random rand = new Random();

    int p = 0;
    int frame = 15;
    List<List<EnemyMovement>> enemyPatterns = new List<List<EnemyMovement>>();
    List<EnemyMovement> lastPattern = new List<EnemyMovement>();
    List<EnemyMovement> enemyHistory = new List<EnemyMovement>();
    private double lastEnemyDirection = 0;

    static void Main(string[] args)
    {
        new F3812FIO().Start();
    }

    F3812FIO() : base(BotInfo.FromFile("F3812FIO.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        MaxRadarTurnRate = 45;
        for (int i = 0; i < 10; i++)
        {
            enemyPatterns.Add(new List<EnemyMovement>());
        }
        while (IsRunning)
        {
            if (!targetLocked)
            {
                SetTurnRadarRight(360);
                SetTurnRight(45);
            }
            GoToEnemy();
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (lockedTargetId == -1 || lockedTargetId == e.ScannedBotId)
        {
            lockedTargetId = e.ScannedBotId;
            double headingChange = NormalizeRelativeAngle(e.Direction - lastEnemyDirection);
            lastEnemyDirection = e.Direction;

            enemyPatterns[lockedTargetId].Add(new EnemyMovement(e.Speed, headingChange, e.Direction, new Point2D(e.X, e.Y)));
            if (enemyPatterns[lockedTargetId].Count > 1000)
                enemyPatterns[lockedTargetId].RemoveAt(0);
            
            lastPattern.Add(new EnemyMovement(e.Speed, headingChange, e.Direction, new Point2D(e.X, e.Y)));
            if (lastPattern.Count > patternLength || lostTargetCount >= maxLostTargetCount)
                lastPattern = new List<EnemyMovement>();

            lostTargetCount = 0;

            double radarTurn = NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
            double extraTurn = Math.Min(Math.Atan(36.0 / DistanceTo(e.X, e.Y)), 45);
            radarTurn = radarTurn + (radarTurn < 0 ? -extraTurn : extraTurn);
            SetTurnRadarLeft(radarTurn);

            //update enemy position (keknya ga perlu lagi)
            enemyPosition = new Point2D(e.X, e.Y);
            enemyDistance = DistanceTo(e.X, e.Y);

            if (enemyDistance < 30)
            {
                frame = 11;
            }
            else if (enemyDistance < 120)
            {
                frame = 15;
            }
            else if (enemyDistance < 200)
            {
                frame = 22;
            }
            else
            {
                frame = 40;
            }

            // kalau history < 70, cukup tembak ke arah musuh, kalau tidak predict
            //tembak hanya ketika lastPattern sudah 7
            Console.WriteLine(enemyHistory.Count);

            if (enemyPatterns[lockedTargetId].Count > 70 && lastPattern.Count == patternLength)
            {
                int matchIndex = FindBestMatch(lastPattern);
                Point2D predictedPos = PredictFuturePosition(matchIndex, new Point2D(e.X, e.Y), e.Direction);
                gunToEnemy(predictedPos.X, predictedPos.Y);
                GoToEnemy();
                if (enemyDistance < 50)
                {
                    SetFire(3);
                }
                else if (enemyDistance < 100)
                {
                    SetFire(2.3);
                }
                else
                {
                    SetFire(1.5);
                }
                Go();
                Console.WriteLine("Predicted");
                Console.WriteLine("Enemy Position: " + e.X + " " + e.Y);
                Console.WriteLine("Predicted Position: " + predictedPos.X + " " + predictedPos.Y);
            }
            else if (lastPattern.Count == patternLength)
            {
                GoToEnemy();
                SetFire(1);
                Go();
                Console.WriteLine("Normal");
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

    public void gunToEnemy(double x, double y)
    {
        double gunTurn = NormalizeRelativeAngle(GunBearingTo(x, y));
        SetTurnGunLeft(gunTurn);
        while (GunTurnRemaining > 0)
        {
            Go();  // Menjalankan turn sampai Gun selesai
        }

    }

    // public List<EnemyMovement> GetLatestPattern()
    // {
    //     int patternLength = 7;
    //     if (enemyHistory.Count < patternLength)
    //         return new List<EnemyMovement>();
    //     return enemyHistory.Skip(enemyHistory.Count - patternLength).ToList();
    // }

    public int FindBestMatch(List<EnemyMovement> pattern)
    {
        int bestMatchIndex = -1;
        double bestScore = double.MaxValue;

        int begin = 0;
        int last = 0;
        if (enemyPatterns[lockedTargetId].Count < 1000)
        {
            last = enemyPatterns[lockedTargetId].Count - lastPattern.Count*2;
            begin = 0;
        }else{
            last = enemyPatterns[lockedTargetId].Count - lastPattern.Count;
            begin = patternLength;
        }
        //lewati history 7 pertama
        for (int i = begin; i < last; i++)
        {
            double score = 0;
            for (int j = 0; j < pattern.Count; j++)
            {
                double velocityDiff = Math.Abs(enemyPatterns[lockedTargetId][i + j].Velocity - lastPattern[j].Velocity);
                double headingChangeDiff = Math.Abs(enemyPatterns[lockedTargetId][i + j].HeadingChange - lastPattern[j].HeadingChange);
                double headingDiff = Math.Abs(enemyPatterns[lockedTargetId][i + j].HeadingDriection - lastPattern[j].HeadingDriection);
                score += velocityDiff + headingDiff + headingDiff; // Total selisih untuk semua tick dalam pola
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestMatchIndex = i;
            }
        }

        return bestMatchIndex;
    }

    public Point2D PredictFuturePosition(int matchIndex, Point2D currentPos, double currentHeading)
    {
        if (matchIndex == -1)
            return new Point2D(300, 400);

        Point2D predictedPos = new Point2D(currentPos.X, currentPos.Y);
        double predictedHeading = currentHeading;

        int i = matchIndex + patternLength;
        double velocity = enemyPatterns[lockedTargetId][i].Velocity;
        double headingChange = enemyPatterns[lockedTargetId][i].HeadingChange;

        if (matchIndex + patternLength + frame >= enemyPatterns[lockedTargetId].Count){
            p = matchIndex + patternLength + frame - enemyPatterns[lockedTargetId].Count;
        } else {
            p = matchIndex + patternLength + frame;
        }
        Console.WriteLine(p + " frame");
        Point2D displacement = enemyPatterns[lockedTargetId][p].Position - enemyPatterns[lockedTargetId][matchIndex + patternLength].Position;
        predictedPos = enemyPosition + displacement;
        // for (int i = matchIndex + 7; i < enemyHistory.Count; i++)
        // {
        //     double velocity = enemyHistory[i].Velocity;
        //     double headingChange = enemyHistory[i].HeadingChange;

        //     predictedHeading = NormalizeRelativeAngle(predictedHeading + headingChange);
        //     predictedPos = new Point2D(
        //         predictedPos.X + velocity * Math.Cos(predictedHeading * (Math.PI / 180)),
        //         predictedPos.Y + velocity * Math.Sin(predictedHeading * (Math.PI / 180))
        //     );
        // }

        return predictedPos;
    }



    // public void KeepDistance()
    // {
        
    // }

    public void GoToPosition(double x, double y)
    {
        double bodyTurn = NormalizeRelativeAngle(BearingTo(x, y));
        double distance = DistanceTo(x, y);

        double gunTurn = NormalizeRelativeAngle(GunBearingTo(x, y));
        SetTurnGunLeft(gunTurn);
        SetTurnLeft((bodyTurn - 45) + (45 * new Random().Next(2)));
        SetForward((distance -100));
    }


    public void GoToEnemy()
    {
        GoToPosition(enemyPosition.X, enemyPosition.Y);
    }


    public override void OnHitBot(HitBotEvent e)
    {
        SetBack(50);
    }

    public override void OnHitWall(HitWallEvent e)
    {

    }
    
    // public class TurnCompleteCondition : Condition
    // {
    //     private readonly Bot bot;

    //     public TurnCompleteCondition(Bot bot)
    //     {
    //         this.bot = bot;
    //     }

    //     public override bool Test()
    //     {
    //         return bot.TurnRemaining == 0;
    //     }
    // }


    public override void OnHitByBullet(HitByBulletEvent e)
    {
        
    }
}
