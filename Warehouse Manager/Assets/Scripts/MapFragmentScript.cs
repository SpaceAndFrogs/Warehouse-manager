using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapFragmentScript : MonoBehaviour
{
    [SerializeField]
    MeshRenderer m_Renderer;
    [SerializeField]
    TileTypes tileTypes;
    public void ChangeFragmentTexture(List<List<float>> noiseSamples)
    {
        Material material = m_Renderer.materials[0];

        Texture2D texture = new Texture2D(noiseSamples.Count,noiseSamples.Count);

        Color32[] pixels = new Color32[noiseSamples.Count * noiseSamples.Count];

        float noiseSample = 0;

        int k = 0;

        for(int i = 0; i < noiseSamples.Count; i ++)
        {
            for(int j = 0; j < noiseSamples.Count; j ++)
            {

                noiseSample = noiseSamples[i][j];
                for (int r = 0; r < tileTypes.tileTypesRanges.Count; r++)
                {
                    if (noiseSample >= tileTypes.tileTypesRanges[r].tileRange.x && noiseSample < tileTypes.tileTypesRanges[r].tileRange.y)
                    {
                        pixels[k] = tileTypes.tileTypesRanges[r].color;
                    }
                }

                k++;
            }
        }


        texture.SetPixels32(pixels);

        texture.Apply();

        material.mainTexture = texture;

    }

    private void OnTriggerEnter(Collider other)
    {
        Button button = other.GetComponent<Button>();

        if (button != null)
        {
            other.gameObject.SetActive(false);
        }
    }
}
