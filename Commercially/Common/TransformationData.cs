using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Commercially.Common
{
    // https://github.com/SONZ-INA/VintageStory-FoodShelves/blob/dev-3.0.0/code/Shared/Transformation/TransformationData.cs
    // Thanks, Sonz-ina
    public class TransformationData(int blockRotation = 0)
    {
        public int index;
        public int shelf;
        public int segment;
        public int item;

        public float preRotate = 0;
        public float x, y, z;
        public float offsetX, offsetY, offsetZ;
        public float rotX, rotY, rotZ;
        public float offsetRotX, offsetRotY, offsetRotZ;
        public float scaleX, scaleY, scaleZ;
        public float offsetOriginX, offsetOriginY, offsetOriginZ;

        public bool hidden;

        public void Reset()
        {
            preRotate = 0;
            x = y = z = 0;
            offsetX = offsetY = offsetZ = 0;
            rotX = rotY = rotZ = 0;
            offsetRotX = offsetRotY = offsetRotZ = 0;
            scaleX = scaleY = scaleZ = 1;
            offsetOriginX = offsetOriginY = offsetOriginZ = 0;
            hidden = false;
        }

        public float[] BuildMatrix()
        {
            Matrixf mat = new();

            if (hidden)
            {
                return mat.Scale(0.01f, 0.01f, 0.01f).Values;
            }

            mat.Translate(0.5f, 0, 0.5f);

            // Handle block rotation
            mat.RotateYDeg(blockRotation + preRotate);

            // Handle segment locations
            mat.Translate(x, y, z);
            mat.Rotate(rotX * GameMath.DEG2RAD, rotY * GameMath.DEG2RAD, rotZ * GameMath.DEG2RAD);

            // Handle item offsets
            mat.Translate(offsetX, offsetY, offsetZ);
            mat.RotateXDeg(offsetRotX);
            mat.RotateYDeg(offsetRotY);
            mat.RotateZDeg(offsetRotZ);
            mat.Translate(offsetOriginX, offsetOriginY, offsetOriginZ);
            mat.Scale(scaleX, scaleY, scaleZ);

            mat.Translate(-0.5f, 0, -0.5f);

            return mat.Values;
        }

    }
}