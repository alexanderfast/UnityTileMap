using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Grid<T> : IEnumerable<T>
{
    [SerializeField]
    private int m_sizeX = -1;

    [SerializeField]
    private int m_sizeY = -1;

    [SerializeField]
    private T[] m_data;

    [SerializeField]
    private T m_default;

    public void SetSize(int x, int y, T defaultValue)
    {
        m_default = defaultValue;

        if (m_sizeX == x && m_sizeY == y)
            return;
        if (x < 1)
            throw new ArgumentException("Size x is less than 1");
        if (y < 1)
            throw new ArgumentException("Size y is less than 1");

        T[] oldData = m_data;

        m_data = Enumerable.Repeat(defaultValue, x * y).ToArray();

        if (oldData != null)
        {
            for (int ix = 0; ix < Mathf.Min(m_sizeX, x); ix++)
            {
                for (int iy = 0; iy < Mathf.Min(m_sizeY, y); iy++)
                {
                    T oldValue = oldData[CoordToIndex(ix, iy)];

                    m_data[(iy * x) + ix] = oldValue;
                }
            }
        }

        m_sizeX = x;
        m_sizeY = y;
    }

    public int Count
    {
        get
        {
            int count = 0;

            for (int i = 0; i < m_data.Length; i++)
            {
                if (! m_data[i].Equals(m_default))
                {
                    count++;
                }
            }
                

            return count;
        }
    }

    public void Clear()
    {
        if (m_data == null)
            return;
        for (int i = 0; i < m_data.Length; i++)
            m_data[i] = m_default;
    }

    public int SizeX
    {
        get { return m_sizeX; }
    }

    public int SizeY
    {
        get { return m_sizeY; }
    }

    public T this[int x, int y]
    {
        get
        {
            Validate(x, y);
            return m_data[CoordToIndex(x, y)];
        }
        set
        {
            Validate(x, y);
            m_data[CoordToIndex(x, y)] = value;
        }
    }

    public bool IsInBounds(int x, int y)
    {
        if (m_data == null)
            throw new InvalidOperationException("Size has not been set");
        if (x < 0 || y < 0 || x >= m_sizeX || y >= m_sizeY)
            return false;
        return true;
    }

    private int CoordToIndex(int x, int y)
    {
        return (y * m_sizeX) + x;
    }

    private void Validate(int x, int y)
    {
        if (m_data == null)
            throw new InvalidOperationException("Size has not been set");
        if (x < 0)
            throw new ArgumentException(string.Format("X ({0}) is less than 0", x));
        if (x >= m_sizeX)
            throw new ArgumentException(string.Format("X ({0}) is greater than size x-1 ({1})", x, m_sizeX));
        if (y < 0)
            throw new ArgumentException(string.Format("Y ({0}) is less than 0", y));
        if (y >= m_sizeY)
            throw new ArgumentException(string.Format("Y ({0}) is greater than size y-1 ({1})", y, m_sizeY));
    }

    #region Implementation of IEnumerable

    public IEnumerator<T> GetEnumerator()
    {
        if (m_data == null)
				return Enumerable.Empty<T>().GetEnumerator();
        return ((IEnumerable<T>)m_data).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
