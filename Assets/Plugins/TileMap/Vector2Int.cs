public class Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }

    public bool Equals(Vector2Int other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return other.x == x && other.y == y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(Vector2Int) && Equals((Vector2Int)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }

    public static bool operator ==(Vector2Int left, Vector2Int right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Vector2Int left, Vector2Int right)
    {
        return !Equals(left, right);
    }
}
