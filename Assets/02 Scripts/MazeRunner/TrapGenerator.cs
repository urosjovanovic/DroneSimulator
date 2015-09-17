using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class TrapGenerator : MonoBehaviour
{
    public Transform LaserPrefab;

    private uint GenerateAheadLimit = 2;
    private Transform drone;
    private Queue<TrapBase> traps;

    // Use this for initialization
    private void Start()
    {
        this.drone = GameObject.Find("DJIPhantom").transform;
        this.traps = new Queue<TrapBase>();
        float offset = this.drone.position.x;
        for (int i = 0; i <= this.GenerateAheadLimit; i++)
            this.traps.Enqueue(this.GenerateNextTrap(offset += 30));
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (this.traps.Count > 0)
        {
            float maxOffset = float.MinValue;

            foreach (var trap in this.traps)
            {
                trap.Translate(-Vector3.right * 0.1f);
                if (trap.Position.x > maxOffset)
                    maxOffset = trap.Position.x;
            }

            var nextTrap = this.traps.Peek();
            if (nextTrap.Position.x < this.drone.position.x)
                if (this.traps.Count <= this.GenerateAheadLimit)
                    this.traps.Enqueue(this.GenerateNextTrap(maxOffset + 30)); //TODO: Staviti random pomeraj

            if (nextTrap.Position.x < this.drone.position.x - 10)
            {
                this.traps.Dequeue();
                nextTrap.Destroy();
            }
        }
    }

    private TrapBase GenerateNextTrap(float offset)
    {
        var randomType = (TrapType)Random.Range(0, Enum.GetValues(typeof(TrapType)).Length - 1);

        switch (randomType)
        {
            case TrapType.HorizontalLaserTrap:
                return new HorizontalLaserTrap(offset, this.LaserPrefab, 0.5f);
            default:
                return null;
        }
    }

    #region Nested type: TrapType

    private enum TrapType
    {
        HorizontalLaserTrap = 0
    }

    #endregion

    #region Nested type: TrapBase

    private abstract class TrapBase
    {
        public abstract void Destroy();

        #region Properties

        public abstract Vector3 Position { get; }

        public abstract TrapType Type { get; }

        #endregion

        public abstract void Translate(Vector3 amount);
    }

    #endregion

    #region Nested type: HorizontalLaserTrap

    private sealed class HorizontalLaserTrap : TrapBase
    {
        private List<Transform> lasers;

        public HorizontalLaserTrap(float offset, Transform laserPrefab, float step)
        {
            this.lasers = new List<Transform>();

            var laserCount = (int)(TerrainGenerator.minWallHeight / step);
            var position = new Vector3(offset, TerrainGenerator.minWallHeight, 0);
            for (int i = 0; i < laserCount; i++)
            {
                var laser = Instantiate(laserPrefab);
                laser.transform.position = position;
                laser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth - 2, 0.01f);
                laser.Rotate(new Vector3(90, 0, 0), Space.Self);
                this.lasers.Add(laser);
                position += new Vector3(0, -step, 0);
            }
        }

        public override void Destroy()
        {
            foreach (var laser in this.lasers)
                Object.Destroy(laser.gameObject);
            this.lasers.Clear();
            this.lasers = null;
        }

        #region Properties

        public override Vector3 Position
        {
            get { return this.lasers[0].position; }
        }

        public override TrapType Type
        {
            get { return TrapType.HorizontalLaserTrap; }
        }

        #endregion

        public override void Translate(Vector3 amount)
        {
            foreach (var laser in this.lasers)
                laser.Translate(amount);
        }
    }

    #endregion
}