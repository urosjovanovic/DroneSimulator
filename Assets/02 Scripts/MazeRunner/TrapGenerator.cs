using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class TrapGenerator : MonoBehaviour
{
    public Transform LaserPrefab;
    public Transform DroneSoldierPrefab;

    private uint GenerateAheadLimit = 5;
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
                trap.Translate(-Vector3.right * TerrainGenerator.terrainMovementSpeed);
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
        var randomType = (TrapType)Random.Range(0, Enum.GetValues(typeof(TrapType)).Length);

        switch (randomType)
        {
            case TrapType.HorizontalLaserTrap:
                return new HorizontalLaserTrap(offset, this.LaserPrefab, 0.5f);
            case TrapType.DiamondLaserTrap:
                return new DiamondLaserTrap(offset, this.LaserPrefab, 0.5f);
            case TrapType.HorizontalMovingLaserTrap:
                return new HorizontalMovingLaserTrap(offset, this.LaserPrefab, 0.25f, Random.Range(0.02f, 0.05f), Random.Range(0, 2) == 1); //TODO: da bude brze sa vremenom
            case TrapType.VerticalColumnTrap:
                return new VerticalColumnTrap(offset, Random.Range((-TerrainGenerator.roadwayWidth / 2) * 0.5f, TerrainGenerator.roadwayWidth / 2) * 0.5f);
            case TrapType.MovingWallTrap:
                return new MovingWallTrap(offset, Random.Range(0, 2) == 1);
            case TrapType.DroneArmyTrap:
                return new DroneArmyTrap(offset, this.DroneSoldierPrefab, DroneArmyTrap.FormationType.DeathJaws);
            default:
                return null;
        }
    }

    #region Nested type: TrapType

    private enum TrapType
    {
        HorizontalLaserTrap = 0,
        DiamondLaserTrap = 1,
        HorizontalMovingLaserTrap = 2,
        VerticalColumnTrap = 3,
        MovingWallTrap = 4,
        DroneArmyTrap = 5
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
                laser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth / 2, 0.01f);
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

    #region Nested type: DiamondLaserTrap

    private sealed class DiamondLaserTrap : TrapBase
    {
        private List<Transform> lasers;

        public DiamondLaserTrap(float offset, Transform laserPrefab, float step)
        {
            this.lasers = new List<Transform>();

            var laserCount = (int)(TerrainGenerator.roadwayWidth / step);
            var position = new Vector3(offset, TerrainGenerator.minWallHeight - 1, 0);
            for (int i = 0; i < laserCount; i++)
            {
                var leftLaser = Instantiate(laserPrefab);
                leftLaser.transform.position = position;
                leftLaser.Rotate(new Vector3(75, 0, 0), Space.Self);
                leftLaser.localScale = new Vector3(0.01f, (float)Math.Sqrt(2 * TerrainGenerator.roadwayWidth * TerrainGenerator.roadwayWidth) / 2, 0.01f);

                this.lasers.Add(leftLaser);

                var rightLaser = Instantiate(laserPrefab);
                rightLaser.transform.position = position;
                rightLaser.Rotate(new Vector3(-75, 0, 0), Space.Self);
                rightLaser.localScale = new Vector3(0.01f, (float)Math.Sqrt(2 * TerrainGenerator.roadwayWidth * TerrainGenerator.roadwayWidth) / 2, 0.01f);

                this.lasers.Add(rightLaser);

                if (i < laserCount / 6)
                {
                    var laser = Instantiate(laserPrefab);
                    laser.transform.position = position + new Vector3(0, 0.7f, 0);
                    laser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth / 2, 0.01f);
                    laser.Rotate(new Vector3(90, 0, 0), Space.Self);
                    this.lasers.Add(laser);
                }

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
            get { return TrapType.DiamondLaserTrap; }
        }

        #endregion

        public override void Translate(Vector3 amount)
        {
            foreach (var laser in this.lasers)
                laser.Translate(amount);
        }
    }

    #endregion

    #region Nested type: HorizontalMovingLaserTrap

    private sealed class HorizontalMovingLaserTrap : TrapBase
    {
        private const int numberOfLines = 3;
        private List<Transform> topLasers;
        private List<Transform> bottomLasers;

        private float currentMoveOffset;
        private Vector3 moveDirection;
        private float speed;
        private bool sameDirection;
        private float moveOffset;

        public HorizontalMovingLaserTrap(float offset, Transform laserPrefab, float step, float speed, bool sameDirection)
        {
            this.topLasers = new List<Transform>();
            this.bottomLasers = new List<Transform>();

            var position = new Vector3(offset, TerrainGenerator.minWallHeight, 0);
            for (int i = 0; i < numberOfLines; i++)
            {
                var topLaser = Instantiate(laserPrefab);
                topLaser.transform.position = position;
                topLaser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth / 2, 0.01f);
                topLaser.Rotate(new Vector3(90, 0, 0), Space.Self);
                this.topLasers.Add(topLaser);

                var bottomLaser = Instantiate(laserPrefab);
                bottomLaser.transform.position = new Vector3(position.x, (sameDirection ? 1.0f : 0.5f) + step * i, position.z);
                bottomLaser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth / 2, 0.01f);
                bottomLaser.Rotate(new Vector3(90, 0, 0), Space.Self);
                this.bottomLasers.Add(bottomLaser);

                position += new Vector3(0, -step, 0);
            }

            this.speed = speed;
            this.sameDirection = sameDirection;
            this.moveOffset = !this.sameDirection ? TerrainGenerator.minWallHeight - step * (numberOfLines + 1) : TerrainGenerator.minWallHeight / 2;
            this.currentMoveOffset = 0.0f;
            this.moveDirection = new Vector3(0, 0, 1);
        }

        public override void Destroy()
        {
            foreach (var laser in this.topLasers)
                Object.Destroy(laser.gameObject);
            this.topLasers.Clear();
            this.topLasers = null;
            foreach (var laser in this.bottomLasers)
                Object.Destroy(laser.gameObject);
            this.bottomLasers.Clear();
            this.bottomLasers = null;
        }

        #region Properties

        public override Vector3 Position
        {
            get { return this.topLasers[0].position; }
        }

        public override TrapType Type
        {
            get { return TrapType.HorizontalMovingLaserTrap; }
        }

        #endregion

        public override void Translate(Vector3 amount)
        {
            if (this.currentMoveOffset > this.moveOffset || this.currentMoveOffset < 0)
                this.moveDirection = new Vector3(this.moveDirection.x, this.moveDirection.y, -1 * this.moveDirection.z);

            float localSpeed = this.speed;

            foreach (var laser in this.topLasers)
            {
                laser.Translate(amount);
                laser.Translate(this.moveDirection * localSpeed);
            }

            if (!this.sameDirection)
                localSpeed *= -1;

            foreach (var laser in this.bottomLasers)
            {
                laser.Translate(amount);
                laser.Translate(this.moveDirection * localSpeed);
            }

            this.currentMoveOffset += this.moveDirection.z * this.speed;
        }
    }

    #endregion

    #region Nested type: VerticalColumnTrap

    private sealed class VerticalColumnTrap : TrapBase
    {
        private Transform column;

        public VerticalColumnTrap(float offset, float offsetHorizontal)
        {
            this.column = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            this.column.position = new Vector3(offset, -TerrainGenerator.minWallHeight, offsetHorizontal);
            this.column.localScale = new Vector3(1, TerrainGenerator.minWallHeight / 2, 1);
        }

        public override void Destroy()
        {
            Object.Destroy(this.column.gameObject);
            this.column = null;
        }

        #region Properties

        public override Vector3 Position
        {
            get { return this.column.position; }
        }

        public override TrapType Type
        {
            get { return TrapType.VerticalColumnTrap; }
        }

        #endregion

        public override void Translate(Vector3 amount)
        {
            this.column.Translate(amount);
            if (this.column.position.y < TerrainGenerator.minWallHeight / 2)
                this.column.Translate(new Vector3(0, 0.1f, 0));
        }
    }

    #endregion

    #region Nested type: MovingWallTrap

    private sealed class MovingWallTrap : TrapBase
    {
        private bool leftSide;
        private Transform wall;

        public MovingWallTrap(float offset, bool leftSide)
        {
            this.leftSide = leftSide;
            this.wall = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            this.wall.position = new Vector3(offset, TerrainGenerator.minWallHeight / 2, (this.leftSide ? 1 : -1) * TerrainGenerator.roadwayWidth);
            this.wall.localScale = new Vector3(2, TerrainGenerator.minWallHeight, TerrainGenerator.roadwayWidth);
        }

        public override void Destroy()
        {
            Object.Destroy(this.wall.gameObject);
            this.wall = null;
        }

        #region Properties

        public override Vector3 Position
        {
            get { return this.wall.position; }
        }

        public override TrapType Type
        {
            get { return TrapType.MovingWallTrap; }
        }

        #endregion

        public override void Translate(Vector3 amount)
        {
            this.wall.Translate(amount);
            var movementVec = new Vector3(0, 0, 0.1f);
            if (this.leftSide)
            {
                if (this.wall.position.z > TerrainGenerator.roadwayWidth / 2.3)
                    this.wall.Translate(-movementVec);
            }
            else
            {
                if (this.wall.position.z < -TerrainGenerator.roadwayWidth / 2.3)
                    this.wall.Translate(movementVec);
            }
        }
    }

    #endregion

    #region Nested type: DroneArmyTrap

    private sealed class DroneArmyTrap : TrapBase
    {
        #region Enums

        public enum FormationType
        {
            DeathJaws = 0
        }

        #endregion

        private List<Transform> drones;
        private FormationType formationType;

        public DroneArmyTrap(float offset, Transform droneSoldierPrefab, FormationType formationType)
        {
            this.drones = new List<Transform>();
            this.formationType = formationType;
            foreach (var position in this.GetPositions(offset, formationType))
            {
                var drone = Instantiate(droneSoldierPrefab).transform;
                drone.position = position;
                drone.forward = new Vector3(-1, 0, 0);
                drone.Rotate(new Vector3(20, 0, 0), Space.Self);
                this.drones.Add(drone);
            }
        }

        public override void Destroy()
        {
            foreach (var drone in this.drones)
                Object.Destroy(drone.gameObject);
            this.drones.Clear();
            this.drones = null;
        }

        #region Properties

        public override Vector3 Position
        {
            get { return this.drones[0].position; }
        }

        public override TrapType Type
        {
            get { return TrapType.DroneArmyTrap; }
        }

        #endregion

        public override void Translate(Vector3 amount)
        {
            foreach (var drone in this.drones)
            {
                drone.Translate(amount, Space.World);
                if (this.formationType == FormationType.DeathJaws)
                {
                    var sin = (float)Math.Sin(Time.frameCount * Time.deltaTime);
                    this.drones[0].position = new Vector3(this.drones[0].position.x, TerrainGenerator.minWallHeight / 2 + 1 + sin, sin - 1.25f);
                    this.drones[1].position = new Vector3(this.drones[1].position.x, TerrainGenerator.minWallHeight / 2 + 1 + sin, -sin + 1.25f);

                    this.drones[2].position = new Vector3(this.drones[2].position.x, TerrainGenerator.minWallHeight / 2 + 1 - sin, 0);
                    this.drones[3].position = new Vector3(this.drones[3].position.x, 2 - sin, 0);

                    this.drones[4].position = new Vector3(this.drones[4].position.x, TerrainGenerator.minWallHeight / 2 - 1 - sin, -sin - 1.25f);
                    this.drones[5].position = new Vector3(this.drones[5].position.x, TerrainGenerator.minWallHeight / 2 - 1 - sin, sin + 1.25f);
                }
            }
        }

        private IEnumerable<Vector3> GetPositions(float offset, FormationType formationType)
        {
            switch (formationType)
            {
                case FormationType.DeathJaws:
                    {
                        yield return new Vector3(offset, TerrainGenerator.minWallHeight / 2 + 1, -1);
                        yield return new Vector3(offset, TerrainGenerator.minWallHeight / 2 + 1, 1);

                        yield return new Vector3(offset, TerrainGenerator.minWallHeight / 2, 0);
                        yield return new Vector3(offset, 1, 0);

                        yield return new Vector3(offset, TerrainGenerator.minWallHeight / 2 - 1, -1);
                        yield return new Vector3(offset, TerrainGenerator.minWallHeight / 2 - 1, 1);

                        break;
                    }
                default:
                    break;
            }
        }
    }

    #endregion
}