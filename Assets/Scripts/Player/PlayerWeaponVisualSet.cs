using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Player Weapon Visual Set", fileName = "PlayerWeaponVisualSet")]
public sealed class PlayerWeaponVisualSet : ScriptableObject
{
    [SerializeField] private WeaponDefinition weapon;
    [Header("Idle")]
    [SerializeField] private Sprite idleDown;
    [SerializeField] private Sprite idleRight;
    [SerializeField] private Sprite idleUp;
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite[] idleDownFrames;
    [SerializeField] private Sprite[] idleRightFrames;
    [SerializeField] private Sprite[] idleUpFrames;
    [SerializeField] private Sprite[] idleLeftFrames;
    [Header("Walk")]
    [SerializeField] private Sprite walkDown;
    [SerializeField] private Sprite walkRight;
    [SerializeField] private Sprite walkUp;
    [SerializeField] private Sprite walkLeft;
    [SerializeField] private Sprite[] walkDownFrames;
    [SerializeField] private Sprite[] walkRightFrames;
    [SerializeField] private Sprite[] walkUpFrames;
    [SerializeField] private Sprite[] walkLeftFrames;
    [Header("Basic attack")]
    [SerializeField] private Sprite attackDown;
    [SerializeField] private Sprite attackRight;
    [SerializeField] private Sprite attackUp;
    [SerializeField] private Sprite attackLeft;
    [SerializeField] private Sprite[] attackDownFrames;
    [SerializeField] private Sprite[] attackRightFrames;
    [SerializeField] private Sprite[] attackUpFrames;
    [SerializeField] private Sprite[] attackLeftFrames;
    [Header("Skill 1")]
    [SerializeField] private Sprite skill1Down;
    [SerializeField] private Sprite skill1Right;
    [SerializeField] private Sprite skill1Up;
    [SerializeField] private Sprite skill1Left;
    [SerializeField] private Sprite[] skill1DownFrames;
    [SerializeField] private Sprite[] skill1RightFrames;
    [SerializeField] private Sprite[] skill1UpFrames;
    [SerializeField] private Sprite[] skill1LeftFrames;
    [Header("Skill 2")]
    [SerializeField] private Sprite skill2Down;
    [SerializeField] private Sprite skill2Right;
    [SerializeField] private Sprite skill2Up;
    [SerializeField] private Sprite skill2Left;
    [SerializeField] private Sprite[] skill2DownFrames;
    [SerializeField] private Sprite[] skill2RightFrames;
    [SerializeField] private Sprite[] skill2UpFrames;
    [SerializeField] private Sprite[] skill2LeftFrames;

    public WeaponDefinition Weapon => weapon;

    public Sprite GetIdle(Vector2 facing) => Pick(facing, idleDown, idleRight, idleUp, idleLeft);
    public Sprite GetWalk(Vector2 facing) => Pick(facing, walkDown, walkRight, walkUp, walkLeft);
    public Sprite GetAttack(Vector2 facing) => Pick(facing, attackDown, attackRight, attackUp, attackLeft);
    public Sprite GetSkill1(Vector2 facing) => Pick(facing, skill1Down, skill1Right, skill1Up, skill1Left);
    public Sprite GetSkill2(Vector2 facing) => Pick(facing, skill2Down, skill2Right, skill2Up, skill2Left);

    public Sprite[] GetIdleFrames(Vector2 facing) => Pick(facing, idleDownFrames, idleRightFrames, idleUpFrames, idleLeftFrames);
    public Sprite[] GetWalkFrames(Vector2 facing) => Pick(facing, walkDownFrames, walkRightFrames, walkUpFrames, walkLeftFrames);
    public Sprite[] GetAttackFrames(Vector2 facing) => Pick(facing, attackDownFrames, attackRightFrames, attackUpFrames, attackLeftFrames);
    public Sprite[] GetSkill1Frames(Vector2 facing) => Pick(facing, skill1DownFrames, skill1RightFrames, skill1UpFrames, skill1LeftFrames);
    public Sprite[] GetSkill2Frames(Vector2 facing) => Pick(facing, skill2DownFrames, skill2RightFrames, skill2UpFrames, skill2LeftFrames);

    private static Sprite Pick(Vector2 facing, Sprite down, Sprite right, Sprite up, Sprite left)
    {
        if (Mathf.Abs(facing.y) > Mathf.Abs(facing.x)) return facing.y >= 0f ? up : down;
        return facing.x >= 0f ? right : left;
    }

    private static Sprite[] Pick(Vector2 facing, Sprite[] down, Sprite[] right, Sprite[] up, Sprite[] left)
    {
        if (Mathf.Abs(facing.y) > Mathf.Abs(facing.x)) return facing.y >= 0f ? up : down;
        return facing.x >= 0f ? right : left;
    }
}
