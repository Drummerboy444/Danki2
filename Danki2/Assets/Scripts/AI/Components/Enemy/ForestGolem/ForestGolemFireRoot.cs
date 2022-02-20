using UnityEngine;

public class ForestGolemFireRoot : IStateMachineComponent
{
    private readonly ForestGolem forestGolem;
    private readonly float minDistance;
    private readonly float maxDistance;

    public ForestGolemFireRoot(ForestGolem forestGolem, float minDistance, float maxDistance)
    {
        this.forestGolem = forestGolem;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }

    public void Enter()
    {
        if (Random.value < 0.1)
        {
            forestGolem.SpawnEnt(GetNewPosition());
        }
        else
        {
            forestGolem.FireRoot(GetNewPosition());
        }
    }

    public void Exit() {}
    public void Update() {}

    private Vector3 GetNewPosition()
    {
        while (true)
        {
            float distance = Random.Range(minDistance, maxDistance);
            Vector2 direction2D = Random.insideUnitCircle.normalized;
            Vector3 direction = new Vector3(direction2D.x, 0f, direction2D.y);
            Vector3 newPosition = ActorCache.Instance.Player.transform.position + distance * direction;
            float newPositionY = Terrain.activeTerrain.SampleHeight(newPosition);
            newPosition.y = newPositionY;
            if (forestGolem.MovementManager.CanReach(newPosition))
            {
                return newPosition;
            }
        }
    }
}
