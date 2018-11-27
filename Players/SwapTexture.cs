using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwapTexture : MonoBehaviour
{
    public SkinnedMeshRenderer[] originalMaterials_01;
    public SkinnedMeshRenderer[] originalMaterials_02;
    public SkinnedMeshRenderer[] originalMaterials_03;

    public Material newMaterial01;
    public Material newMaterial02;
    public Material newMaterial03;

    public void OnSetOtherTexture()
    {
        if(newMaterial01 != null)
        {
            for (int i = 0; i < originalMaterials_01.Length; i++)
            {
                originalMaterials_01[i].material = newMaterial01;
            }
        }

        if(newMaterial02 != null)
        {
            for (int i = 0; i < originalMaterials_02.Length; i++)
            {
                originalMaterials_02[i].material = newMaterial02;
            }
        }

        if (newMaterial03 != null)
        {
            for (int i = 0; i < originalMaterials_03.Length; i++)
            {
                originalMaterials_03[i].material = newMaterial03;
            }
        }
    }
}
