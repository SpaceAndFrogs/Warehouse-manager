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
        transform.rotation = Quaternion.Euler(0, 0, 0);

        Material material = m_Renderer.materials[0];

        Texture2D texture = new Texture2D(noiseSamples.Count,noiseSamples.Count);

        float noiseSample;

        for(int i = 0; i < noiseSamples.Count; i ++)
        {
            for(int j = 0; j < noiseSamples.Count; j ++)
            {

                noiseSample = noiseSamples[i][j];
                for (int r = 0; r < tileTypes.tileTypesRanges.Count; r++)
                {
                    if (noiseSample >= tileTypes.tileTypesRanges[r].tileRange.x && noiseSample < tileTypes.tileTypesRanges[r].tileRange.y)
                    {
                        texture.SetPixel(i, j, tileTypes.tileTypesRanges[r].color);
                    }
                }

            }
        }

        texture.filterMode = FilterMode.Point;

        texture.Apply();

        material.mainTexture = texture;

        transform.rotation = Quaternion.Euler(0, 180, 0);

    }

    public void ChangeTileOnMap(Vector2 tileCords, Color tileColor)
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        Material material = m_Renderer.materials[0];

        Texture2D texture = (Texture2D)material.mainTexture;

        texture.SetPixel((int)tileCords.x, (int)tileCords.y, tileColor);

        texture.Apply();

        material.mainTexture = texture;

        transform.rotation = Quaternion.Euler(0, 180, 0);
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
