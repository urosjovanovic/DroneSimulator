using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public const float terrainMovementSpeed = 0.2f;
    public const float roadwayLength = 200;
    public const float roadwayWidth = 5;
    public const float minWallHeight = 5;
    public const float maxWallHeight = 10;
    public const float minWallWidth = 5;
    public const float maxWallWidth = 10;

    private uint GenerateAheadLimit = 2;
    private Transform drone;
    private Queue<TerrainSegment> terrainSegments;

    // Use this for initialization
    private void Start()
    {
        this.drone = GameObject.Find("DJIPhantom").transform;
        this.terrainSegments = new Queue<TerrainSegment>();
        var position = new Vector3(-roadwayLength / 2, 0, 0);
        for (int i = 0; i <= this.GenerateAheadLimit; i++)
            this.terrainSegments.Enqueue(new TerrainSegment(position + new Vector3(roadwayLength * (i + 1), 0, 0)));
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        //Debug.Log(this.terrainSegments.Count);
        if (this.terrainSegments.Count > 0)
        {
            var activeSegment = this.terrainSegments.Peek();

            if (activeSegment.Position.x < this.drone.position.x)
                if (this.terrainSegments.Count <= this.GenerateAheadLimit)
                    this.terrainSegments.Enqueue(new TerrainSegment(activeSegment.Position + new Vector3(roadwayLength * this.terrainSegments.Count, 0, 0)));

            if (activeSegment.Position.x + roadwayLength / 2 < this.drone.position.x - 10)
            {
                this.terrainSegments.Dequeue();
                activeSegment.Destroy();
            }

            foreach (var segment in this.terrainSegments)
                segment.Translate(-Vector3.right * terrainMovementSpeed);
        }
    }

    #region Nested type: TerrainSegment

    private sealed class TerrainSegment
    {
        private Transform roadway;
        private List<Transform> leftWalls;
        private List<Transform> rightWalls;

        public TerrainSegment(Vector3 position)
        {
            this.roadway = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            this.roadway.position = position;
            this.roadway.localScale = new Vector3(roadwayLength, 1, roadwayWidth);

            this.leftWalls = new List<Transform>();
            float wallWidthSum = 0;
            while (wallWidthSum < roadwayLength)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                float width = Random.Range(minWallWidth, maxWallWidth);
                float height = Random.Range(minWallHeight, maxWallHeight);
                wall.position = this.roadway.position - new Vector3(roadwayLength / 2, 0, 0) + new Vector3(wallWidthSum + width / 2, height / 2, -roadwayWidth / 2 - 0.5f);
                wall.localScale = new Vector3(width, height, 1);
                this.leftWalls.Add(wall);
                wallWidthSum += width;
            }

            this.rightWalls = new List<Transform>();
            wallWidthSum = 0;
            while (wallWidthSum < roadwayLength)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                float width = Random.Range(minWallWidth, maxWallWidth);
                float height = Random.Range(minWallHeight, maxWallHeight);
                wall.position = this.roadway.position - new Vector3(roadwayLength / 2, 0, 0) + new Vector3(wallWidthSum + width / 2, height / 2, roadwayWidth / 2 + 0.5f);
                wall.localScale = new Vector3(width, height, 1);
                this.rightWalls.Add(wall);
                wallWidthSum += width;
            }
        }

        public void Destroy()
        {
            Object.Destroy(this.roadway.gameObject);
            foreach (var wall in this.leftWalls)
                Object.Destroy(wall.gameObject);
            this.leftWalls.Clear();
            foreach (var wall in this.rightWalls)
                Object.Destroy(wall.gameObject);
            this.rightWalls.Clear();
            this.roadway = null;
            this.leftWalls = null;
            this.rightWalls = null;
        }

        #region Properties

        public Vector3 Position
        {
            get { return this.roadway.position; }
        }

        #endregion

        public void Translate(Vector3 amount)
        {
            this.roadway.Translate(amount);
            foreach (var wall in this.leftWalls)
                wall.Translate(amount);
            foreach (var wall in this.rightWalls)
                wall.Translate(amount);
        }
    }

    #endregion
}