using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Horizon;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using ImGuiNET;
using ObjLoader.Loader.Data;
using ObjLoader.Loader.Data.VertexData;
using Silk.NET.Maths;

namespace Gravitator2D;

public class Universe : Entity
{
    public const int MAX_BODIES_COUNT = 16;

    public int BodyCount { get; private set; }

    private CircleBody[] physicsBodies;
    private LineBody bottomLine;
    public Body[] RenderBodies;

    public ReaderWriterLockSlim Lock { get; init; }
    public Vector2 Range = new Vector2(5f);

    public float TimeScale = 1f;
    public float GravityConstant = 0.0001f;
    public float CentralAttractionStrength = 0.001f;
    public float CoRestitution = 0.6f;
    public float RepulsionStrength = 10.0f; // Adjusted for the smaller radius
    public float MaxSpeed = 400.0f; // Adjusted for the smaller scale

    private static readonly Random rand = new();

    public Universe()
    {
        Lock = new ReaderWriterLockSlim();
        physicsBodies = new CircleBody[MAX_BODIES_COUNT];
        RenderBodies = new Body[MAX_BODIES_COUNT];

        Debug.Assert(Marshal.SizeOf<Body>() % 16 == 0);

        bottomLine = new LineBody(new Vector2(-5, 0), new Vector2(5, -8));

        AddComponent<CustomBehaviourComponent>().OnUpdate += TryUpdateBodies;
        AddEntity(new BodyRenderer(this));

        for (int i = 0; i < MAX_BODIES_COUNT; i++)
        {
            physicsBodies[i] = new CircleBody
            {
                Position = new Vector2(rand.NextSingle() * 10 - 5, 5),
                Radius = rand.NextSingle() * .5f + .5f,
                Color = GetRandomVector3()
            };
            physicsBodies[i].ApplyForce(new Vector2(0, -9.8f * 10.0f));

            RenderBodies[i] = physicsBodies[i].GetRenderBody();
        }
        BodyCount = MAX_BODIES_COUNT;
    }

    [Pure]
    static float map(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }

    private void TryUpdateBodies(float dt)
    {
        bool jump = GameManager.Instance.InputManager
            .GetVirtualController()
            .IsPressed(Horizon.Input.VirtualAction.Interact);

        for (int i = 0; i < MAX_BODIES_COUNT; i++)
        {
            var body = physicsBodies[i];

            if (bottomLine.Collides(body))
            {
                body.ApplyForce(-body.GetForce() * body.Damping);
                body.Update(physicsBodies, dt);
            }
            body.Update(physicsBodies, dt);
            RenderBodies[i] = body.GetRenderBody();
        }
    }

    private void UpdateBodies(float stepInterval)
    {
        for (int i = 0; i < MAX_BODIES_COUNT; i++)
        {
            //physicsBodies[i].Position = new Vector2(i);
        }
    }

    private static Vector3 GetRandomVector3() =>
        new(
            rand.NextSingle() * 0.8f + 0.2f,
            rand.NextSingle() * 0.8f + 0.2f,
            rand.NextSingle() * 0.8f + 0.2f
        );

    private static Vector2 GetRandomVector2() =>
        new(rand.NextSingle() * 2.0f - 1.0f, rand.NextSingle() * 2.0f - 1.0f);
}
