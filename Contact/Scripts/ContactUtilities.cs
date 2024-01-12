using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vowgan.Contact
{
    public class ContactUtilities : MonoBehaviour
    {


        public static int[][] CaptureMaterialIds(Material[][] materials)
        {
            int[][] materialIds = new int[materials.Length][];
            
            for (int x = 0; x < materialIds.Length; x++)
            {
                materialIds[x] = new int[materials[x].Length];
                for (int y = 0; y < materials[x].Length; y++)
                {
                    Material mat = materials[x][y];
                    if (!mat) continue;
                    materialIds[x][y] = mat.GetInstanceID();
                }
            }

            return materialIds;
        }
        
    }
}