using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Util
{
    // https://github.com/SONZ-INA/VintageStory-FoodShelves/blob/dev-3.0.0/code/Shared/Transformation/TransformationData.cs
    // Thanks, Sonz-ina
    public class TransformationData()
    {
        public TransformationData globalTransform;

        public float preRotate = 0;
        public float X, Y, Z;
        public float OffsetX, OffsetY, OffsetZ;
        public float RotX, RotY, RotZ;
        public float OffsetRotX, OffsetRotY, OffsetRotZ;
        public float ScaleX = 1, ScaleY = 1, ScaleZ = 1;
        public float OffsetOriginX, OffsetOriginY, OffsetOriginZ;
        public bool hidden;

        public float ScaleXYZ { set { ScaleX = value; ScaleY = value; ScaleZ = value; } }

        /// <summary>
        /// Resets all properties to 0, except preRotate.
        /// </summary>
        public void Reset()
        {
            X = Y = Z = 0;
            OffsetX = OffsetY = OffsetZ = 0;
            RotX = RotY = RotZ = 0;
            OffsetRotX = OffsetRotY = OffsetRotZ = 0;
            ScaleX = ScaleY = ScaleZ = 1;
            OffsetOriginX = OffsetOriginY = OffsetOriginZ = 0;
            hidden = false;
        }

        public float[] BuildMatrix()
        {
            Matrixf mat = new();

            if (hidden)
            {
                return mat.Scale(0.01f, 0.01f, 0.01f).Values;
            }

            if (globalTransform == null)
            {
                globalTransform = new TransformationData();
            }

            mat.Translate(0.5f, 0, 0.5f);

            // Handle block rotation
            mat.RotateYDeg(globalTransform.preRotate + preRotate);

            // Handle segment locations
            mat.Translate(
                globalTransform.X + X,
                globalTransform.Y + Y,
                globalTransform.Z + Z
                );

            mat.Rotate(
                (globalTransform.RotX * GameMath.DEG2RAD) + (RotX * GameMath.DEG2RAD), 
                (globalTransform.RotY * GameMath.DEG2RAD) + (RotY * GameMath.DEG2RAD), 
                (globalTransform.RotZ * GameMath.DEG2RAD) + (RotZ * GameMath.DEG2RAD)
                );

            // Handle item offsets
            mat.Translate(globalTransform.OffsetX + OffsetX, globalTransform.OffsetY + OffsetY, globalTransform.OffsetZ + OffsetZ);
            mat.RotateYDeg(globalTransform.OffsetRotY + OffsetRotY);
            mat.RotateXDeg(globalTransform.OffsetRotX + OffsetRotX);
            mat.RotateZDeg(globalTransform.OffsetRotZ + OffsetRotZ);

            mat.Translate(
                globalTransform.OffsetOriginX + OffsetOriginX,
                globalTransform.OffsetOriginY + OffsetOriginY,
                globalTransform.OffsetOriginZ + OffsetOriginZ
                );

            mat.Scale(
                globalTransform.ScaleX * ScaleX,
                globalTransform.ScaleY * ScaleY,
                globalTransform.ScaleZ * ScaleZ
                );

            mat.Translate(-0.5f, 0, -0.5f);

            return mat.Values;
        }
    }

}
