using System;
using System.Collections.Generic;
using System.Text;

namespace RMGS.Args
{
    public class Constraint
    {
        public Constraint() { }
    }

    public class PresetConstraint : Constraint
    {
        // left / right = (position, rotation)
        public (string, int) Left { get; set; }
        public (string, int) Right { get; set; }

        public PresetConstraint((string, int) left, (string, int) right)
        {
            Left = left;
            Right = right;
        }
        public PresetConstraint(string left, int leftRotation, string right, int rightRotation)
        {
            Left = (left, leftRotation);
            Right = (right, rightRotation);
        }

    }
    public class RealtimeConstraint : Constraint
    {
        public int BanPosition { get; set; }
        public RealtimeConstraint(int banPosition)
        {
            BanPosition = banPosition;
        }
    }
}
