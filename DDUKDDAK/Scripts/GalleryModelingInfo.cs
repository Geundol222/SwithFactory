using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalleryModelingInfo : GalleryObjectInfo
{
    public Material myMat;

    public void SetMyMat(Material mat)
    {
        myMat = mat;
        myMesh.material = myMat;
    }

    public void SetMatTexture(string type, Texture tex)
    {
        myMesh.sharedMaterial.SetTexture(type, tex);
    }
}
