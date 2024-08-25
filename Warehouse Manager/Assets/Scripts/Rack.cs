using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rack : MonoBehaviour
{
    public Items.Item itemOnRack;

    public int amountOfItems = 0;

    public int reservedAmountOfItems = 0;

    public Tile tileWithRack;
    public int desiredAmountOfItems = 0;
    public int maxAmountOfItems = 0;
}
