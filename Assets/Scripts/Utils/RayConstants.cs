using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualMaze.Assets.Scripts.Utils
{
        public static class RayConstants
        {
            // Use this constant to represent an uninitialized or invalid Ray
            public static readonly Ray NullRay = new Ray(Vector3.negativeInfinity, Vector3.negativeInfinity);

            public static bool IsAbsoluteEqual(Ray ray1, Ray ray2)
            {
                // Compare origin points and directions
                return ray1.origin == ray2.origin && ray1.direction == ray2.direction;
            }

        }

}