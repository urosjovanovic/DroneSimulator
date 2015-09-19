using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class TrapGenerator : MonoBehaviour
{
    public Transform LaserPrefab;
    public Transform DroneSoldierPrefab;
    public Transform TimeCollectiblePrefab;

    private uint GenerateAheadLimit = 5;
    private Transform drone;
    private Queue<TrapBase> traps;
    private Queue<Transform> collectibles;
    private int collectibleCountdown;
    private TrapType lastGeneratedTrap;

    // Use this for initialization
    private void Start()
    {
        this.drone = GameObject.Find("DJIPhantom").transform;
        this.traps = new Queue<TrapBase>();
        this.collectibles = new Queue<Transform>();
        this.RecalculateCollectibleCountdown();
        float offset = this.drone.position.x;
        for (int i = 0; i <= this.GenerateAheadLimit; i++)
        {
            if (this.traps.Count > 0)
            {
                if (--collectibleCountdown == 0)
                {
                    this.GenerateCollectible(offset, offset + 30);
                    this.RecalculateCollectibleCountdown();
                }
            }
            this.traps.Enqueue(this.GenerateNextTrap(offset += 30));
        }
        this.lastGeneratedTrap = TrapType.PingPongTrap;
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
                trap.Update();
                if (trap.Offset > maxOffset)
                    maxOffset = trap.Offset;
            }

            foreach (var collectible in this.collectibles)
            {
                collectible.Rotate(Vector3.up, 2, Space.World);
                collectible.Translate(-Vector3.right * TerrainGenerator.terrainMovementSpeed, Space.World);
            }

            var nextTrap = this.traps.Peek();
            if (nextTrap.Offset < this.drone.position.x)
                if (this.traps.Count <= this.GenerateAheadLimit)
                {
                    if (--collectibleCountdown == 0)
                    {
                        this.GenerateCollectible(maxOffset, maxOffset + 30);
                        this.RecalculateCollectibleCountdown();
                    }
                    this.traps.Enqueue(this.GenerateNextTrap(maxOffset + 30)); //TODO: Staviti random pomeraj
                }

            if (this.collectibles.Count > 0)
            {
                var nextCollectible = this.collectibles.Peek();
                if (nextCollectible.position.x < this.drone.position.x - 10)
                {
                    this.collectibles.Dequeue();
                    GameObject.Destroy(nextCollectible.gameObject);
                }
            }

            if (nextTrap.Offset < this.drone.position.x - 10)
            {
                this.traps.Dequeue();
                nextTrap.Destroy();
            }
        }
    }

    private TrapBase GenerateNextTrap(float offset)
    {
        int length = Enum.GetValues(typeof(TrapType)).Length;
        var randomType = (TrapType)Random.Range(0, length);
        if (randomType == this.lastGeneratedTrap)
        {
            if ((int)randomType <= 3)
                randomType = (TrapType)(((int)randomType + 4) % length);
            else
                randomType = (TrapType)(((int)randomType + 1) % length);
        }
        this.lastGeneratedTrap = randomType;
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
            case TrapType.PingPongTrap:
                return new PingPongTrap(offset, Random.Range(0, 4));
            default:
                return null;
        }
    }

    private void GenerateCollectible(float previousTrapOffset, float nextTrapOffset)
    {
        const float minWallHalfHeight = TerrainGenerator.minWallHeight / 2;
        const float roadwayHalfWidth = TerrainGenerator.roadwayWidth / 2;
        var collectible = Instantiate(this.TimeCollectiblePrefab);
        collectible.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        collectible.position = new Vector3((previousTrapOffset + nextTrapOffset) / 2 + Random.Range(-5, 10), minWallHalfHeight + Random.Range(-minWallHalfHeight * 0.5f, minWallHalfHeight * 0.5f), Random.Range(-roadwayHalfWidth * 0.5f, roadwayHalfWidth * 0.5f));
        collectible.name = "Collectible";
        this.collectibles.Enqueue(collectible);
    }

    private void RecalculateCollectibleCountdown()
    {
        this.collectibleCountdown = Random.Range(3, 7);
    }

    #region Nested type: TrapBase

    private abstract class TrapBase
    {
        protected GameObject gameObject;

        protected TrapBase(float offset)
        {
            this.gameObject = new GameObject(this.Type.ToString());
            this.gameObject.transform.position = new Vector3(offset, 0, 0);
        }

        public virtual void Destroy()
        {
            GameObject.Destroy(this.gameObject);
            this.gameObject = null;
        }

        #region Properties

        public abstract TrapType Type { get; }

        public float Offset { get { return this.gameObject.transform.position.x; } }

        #endregion

        public virtual void Update()
        {
            this.gameObject.transform.Translate(-Vector3.right * TerrainGenerator.terrainMovementSpeed, Space.World);
        }
    }

    #endregion

    #region Nested type: HorizontalLaserTrap

    private sealed class HorizontalLaserTrap : TrapBase
    {
        private List<Transform> lasers;

        public HorizontalLaserTrap(float offset, Transform laserPrefab, float step)
            : base(offset)
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
                laser.parent = this.gameObject.transform;
                this.lasers.Add(laser);
                position += new Vector3(0, -step, 0);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            this.lasers.Clear();
            this.lasers = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.HorizontalLaserTrap; }
        }

        #endregion
    }

    #endregion

    #region Nested type: DiamondLaserTrap

    private sealed class DiamondLaserTrap : TrapBase
    {
        private List<Transform> lasers;

        public DiamondLaserTrap(float offset, Transform laserPrefab, float step)
            : base(offset)
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
                leftLaser.parent = this.gameObject.transform;
                this.lasers.Add(leftLaser);

                var rightLaser = Instantiate(laserPrefab);
                rightLaser.transform.position = position;
                rightLaser.Rotate(new Vector3(-75, 0, 0), Space.Self);
                rightLaser.localScale = new Vector3(0.01f, (float)Math.Sqrt(2 * TerrainGenerator.roadwayWidth * TerrainGenerator.roadwayWidth) / 2, 0.01f);
                rightLaser.parent = this.gameObject.transform;
                this.lasers.Add(rightLaser);

                if (i < laserCount / 6)
                {
                    var laser = Instantiate(laserPrefab);
                    laser.transform.position = position + new Vector3(0, 0.7f, 0);
                    laser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth / 2, 0.01f);
                    laser.Rotate(new Vector3(90, 0, 0), Space.Self);
                    laser.parent = this.gameObject.transform;
                    this.lasers.Add(laser);
                }

                position += new Vector3(0, -step, 0);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            this.lasers.Clear();
            this.lasers = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.DiamondLaserTrap; }
        }

        #endregion
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
            : base(offset)
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
                topLaser.parent = this.gameObject.transform;
                this.topLasers.Add(topLaser);

                var bottomLaser = Instantiate(laserPrefab);
                bottomLaser.transform.position = new Vector3(position.x, (sameDirection ? 1.0f : 0.5f) + step * i, position.z);
                bottomLaser.localScale = new Vector3(0.01f, TerrainGenerator.roadwayWidth / 2, 0.01f);
                bottomLaser.Rotate(new Vector3(90, 0, 0), Space.Self);
                bottomLaser.parent = this.gameObject.transform;
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
            base.Destroy();
            this.topLasers.Clear();
            this.topLasers = null;
            this.bottomLasers.Clear();
            this.bottomLasers = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.HorizontalMovingLaserTrap; }
        }

        #endregion

        public override void Update()
        {
            base.Update();

            if (this.currentMoveOffset > this.moveOffset || this.currentMoveOffset < 0)
                this.moveDirection = new Vector3(this.moveDirection.x, this.moveDirection.y, -1 * this.moveDirection.z);

            float localSpeed = this.speed;

            foreach (var laser in this.topLasers)
                laser.Translate(this.moveDirection * localSpeed);

            if (!this.sameDirection)
                localSpeed *= -1;

            foreach (var laser in this.bottomLasers)
                laser.Translate(this.moveDirection * localSpeed);

            this.currentMoveOffset += this.moveDirection.z * this.speed;
        }
    }

    #endregion

    #region Nested type: VerticalColumnTrap

    private sealed class VerticalColumnTrap : TrapBase
    {
        private Transform column;

        public VerticalColumnTrap(float offset, float offsetHorizontal)
            : base(offset)
        {
            this.column = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            this.column.position = new Vector3(offset, -TerrainGenerator.minWallHeight, offsetHorizontal);
            this.column.localScale = new Vector3(1, TerrainGenerator.minWallHeight / 2, 1);
            this.column.GetComponent<Collider>().isTrigger = true;
            this.column.parent = this.gameObject.transform;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.column = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.VerticalColumnTrap; }
        }

        #endregion

        public override void Update()
        {
            base.Update();
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
            : base(offset)
        {
            this.leftSide = leftSide;
            this.wall = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            this.wall.position = new Vector3(offset, TerrainGenerator.minWallHeight / 2, (this.leftSide ? 1 : -1) * TerrainGenerator.roadwayWidth);
            this.wall.localScale = new Vector3(2, TerrainGenerator.minWallHeight, TerrainGenerator.roadwayWidth);
            this.wall.GetComponent<Collider>().isTrigger = true;
            this.wall.parent = this.gameObject.transform;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.wall = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.MovingWallTrap; }
        }

        #endregion

        public override void Update()
        {
            base.Update();
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
            : base(offset)
        {
            this.drones = new List<Transform>();
            this.formationType = formationType;
            foreach (var position in this.GetPositions(offset, formationType))
            {
                var drone = Instantiate(droneSoldierPrefab).transform;
                drone.position = position;
                drone.forward = new Vector3(-1, 0, 0);
                drone.Rotate(new Vector3(20, 0, 0), Space.Self);
                drone.GetComponent<Collider>().isTrigger = true;
                drone.parent = this.gameObject.transform;
                this.drones.Add(drone);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            this.drones.Clear();
            this.drones = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.DroneArmyTrap; }
        }

        #endregion

        public override void Update()
        {
            base.Update();
            if (this.formationType == FormationType.DeathJaws)
            {
                var sin = (float)Math.Sin(Time.frameCount * Time.deltaTime);
                var sin2 = (float)Math.Sin(Time.frameCount * Time.deltaTime * 2.5);
                this.drones[0].position = new Vector3(this.drones[0].position.x, TerrainGenerator.minWallHeight, sin - 1.25f);
                this.drones[1].position = new Vector3(this.drones[1].position.x, TerrainGenerator.minWallHeight, -sin + 1.25f);
                this.drones[2].position = new Vector3(this.drones[2].position.x, TerrainGenerator.minWallHeight / 2 + 1.65f - sin, -1.25f - sin);
                this.drones[3].position = new Vector3(this.drones[3].position.x, TerrainGenerator.minWallHeight / 2 + 1.65f - sin, 1.25f + sin);
                this.drones[4].position = new Vector3(this.drones[4].position.x, TerrainGenerator.minWallHeight / 2 + 0.75f + sin2, 0);
                this.drones[5].position = new Vector3(this.drones[5].position.x, TerrainGenerator.minWallHeight / 2 - 0.45f + sin, -1.25f - sin);
                this.drones[6].position = new Vector3(this.drones[6].position.x, TerrainGenerator.minWallHeight / 2 - 0.45f + sin, 1.25f + sin);
                this.drones[7].position = new Vector3(this.drones[7].position.x, 1, sin - 1.25f);
                this.drones[8].position = new Vector3(this.drones[8].position.x, 1, -sin + 1.25f);
            }
        }

        private IEnumerable<Vector3> GetPositions(float offset, FormationType formationType)
        {
            switch (formationType)
            {
                case FormationType.DeathJaws:
                    return Enumerable.Repeat(new Vector3(offset, 0, 0), 9);
                default:
                    return Enumerable.Empty<Vector3>();
            }
        }
    }

    #endregion

    #region Nested type: PingPongTrap

    private sealed class PingPongTrap : TrapBase
    {
        private static List<Vector3> ballPossitions;
        private const float ballSize = 1;
        private Transform leftPlane;
        private Transform rightPlane;
        private Transform ball;
        private int ballPossition;
        private Vector3 currentBallDirection;

        public PingPongTrap(float offset, int ballPossiion)
            : base(offset)
        {
            if (ballPossitions == null)
            {
                float y = TerrainGenerator.minWallHeight - ballSize / 2;
                float z = (TerrainGenerator.roadwayWidth - (ballSize + 0.05f)) / 2;
                ballPossitions = new List<Vector3> { new Vector3(0, ballSize / 2 + 0.5f, -z), new Vector3(0, y, z), new Vector3(0, y, -z), new Vector3(0, ballSize / 2 + 0.5f, z) };
            }

            this.ballPossition = ballPossiion;

            this.ball = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            this.ball.localScale = new Vector3(ballSize, ballSize, ballSize);
            this.ball.position = new Vector3(offset, ballPossitions[this.ballPossition].y, ballPossitions[this.ballPossition].z);
            this.ball.GetComponent<Collider>().isTrigger = true;
            this.ball.parent = this.gameObject.transform;
            this.currentBallDirection = new Vector3(0, 0, -this.ball.position.z).normalized;

            bool leftSide = this.currentBallDirection.z < 0;
            this.leftPlane = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            this.leftPlane.GetComponent<Renderer>().material.color = Color.red;
            this.leftPlane.position = new Vector3(offset, leftSide ? this.ball.position.y : TerrainGenerator.minWallHeight / 2, -TerrainGenerator.roadwayWidth / 2);
            this.leftPlane.localScale = new Vector3(1, 2, 0.05f);
            this.leftPlane.parent = this.gameObject.transform;

            this.rightPlane = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            this.rightPlane.GetComponent<Renderer>().material.color = Color.blue;
            this.rightPlane.position = new Vector3(offset, !leftSide ? this.ball.position.y : TerrainGenerator.minWallHeight / 2, TerrainGenerator.roadwayWidth / 2);
            this.rightPlane.localScale = new Vector3(1, 2, 0.05f);
            this.rightPlane.parent = this.gameObject.transform;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.leftPlane = null;
            this.rightPlane = null;
            this.ball = null;
        }

        #region Properties

        public override TrapType Type
        {
            get { return TrapType.PingPongTrap; }
        }

        #endregion

        public override void Update()
        {
            base.Update();

            this.ball.Translate(this.currentBallDirection * 0.15f, Space.World);
            if (this.currentBallDirection.z < 0)
                this.leftPlane.position = Vector3.Lerp(this.leftPlane.position, new Vector3(this.leftPlane.position.x, this.ball.position.y, this.leftPlane.position.z), Time.deltaTime * 2);
            else
                this.rightPlane.position = Vector3.Lerp(this.rightPlane.position, new Vector3(this.rightPlane.position.x, this.ball.position.y, this.rightPlane.position.z), Time.deltaTime * 2);

            if (Math.Abs(this.ball.position.z) + ballSize / 2 >= TerrainGenerator.roadwayWidth / 2)
            {
                this.currentBallDirection = new Vector3(0, 0, -this.currentBallDirection.z).normalized;
                if (this.ball.position.y >= TerrainGenerator.minWallHeight * 0.75f)
                    this.currentBallDirection = this.currentBallDirection + new Vector3(0, -Random.Range(0, 0.5f), 0);
                else
                    this.currentBallDirection = this.currentBallDirection + new Vector3(0, Random.Range(0, 0.5f), 0);
            }
        }
    }

    #endregion
}

public enum TrapType
{
    HorizontalLaserTrap = 0,
    DiamondLaserTrap = 1,
    HorizontalMovingLaserTrap = 2,
    VerticalColumnTrap = 3,
    MovingWallTrap = 4,
    DroneArmyTrap = 5,
    PingPongTrap = 6
}