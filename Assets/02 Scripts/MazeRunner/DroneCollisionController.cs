using UnityEngine;
using System.Collections;
using System;

public class DroneCollisionController : MonoBehaviour
{
    private GameTimeController gameTimeController;

    // Use this for initialization
    void Start()
    {
        this.gameTimeController = GameObject.FindObjectOfType<GameTimeController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "Collectible")
        {
            this.gameTimeController.UpdateTimeBy(20);
            other.gameObject.SetActive(false);
        }
        else
        {
            try
            {
                Transform parent = other.gameObject.transform.parent;
                if (parent != null)
                {
                    TrapType trapType = (TrapType)Enum.Parse(typeof(TrapType), parent.name);
                    int value;
                    switch (trapType)
                    {
                        case TrapType.HorizontalLaserTrap:
                            value = -5;
                            break;
                        case TrapType.DiamondLaserTrap:
                            value = -5;
                            break;
                        case TrapType.HorizontalMovingLaserTrap:
                            value = -10;
                            break;
                        case TrapType.VerticalColumnTrap:
                            value = -20;
                            break;
                        case TrapType.MovingWallTrap:
                            value = -20;
                            break;
                        case TrapType.DroneArmyTrap:
                            value = -20;
                            break;
                        case TrapType.PingPongTrap:
                            value = -20;
                            break;
                        default:
                            value = 0;
                            break;
                    }
                    this.gameTimeController.UpdateTimeBy(value);

                    for (int i = 0; i < parent.childCount; i++)
                        parent.GetChild(i).GetComponent<Collider>().enabled = false;
                }
            }
            catch
            {
            }
        }
    }
}
