using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;


public class EnemyMovement
{
    public double Velocity { get; }
    public double HeadingChange { get; }
    public double HeadingDriection { get; }
    public Point2D Position { get; set; }
    public EnemyMovement(double velocity, double headingChange, double heading, Point2D position)
    {
        Velocity = velocity;
        HeadingChange = headingChange;
        HeadingDriection = heading;
        Position = new Point2D(position.X, position.Y);
    }
}

public class Point2D
{
    public double X { get; }
    public double Y { get; }

    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static Point2D operator-(Point2D p1, Point2D p2)
    {
        return new Point2D(p1.X - p2.X, p1.Y - p2.Y);
    }

    public static Point2D operator+(Point2D p1, Point2D p2)
    {
        return new Point2D(p1.X + p2.X, p1.Y + p2.Y);
    }
}
