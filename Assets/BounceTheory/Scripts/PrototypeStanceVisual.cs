using UnityEngine;

namespace BounceTheory
{
    /// <summary>Prototype-only pose readability driven by authoritative stance state.</summary>
    public sealed class PrototypeStanceVisual : MonoBehaviour
    {
        [Header("Stance Source")]
        [SerializeField] private PoundDribbleController stanceSource;

        [Header("Player Primitives")]
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform jerseyStripe;

        [Header("Prototype Pose Tuning")]
        [SerializeField] private float lowHeightOffset = -0.38f;
        [SerializeField, Range(0.5f, 1f)] private float lowVerticalScale = 0.78f;
        [SerializeField, Range(1f, 1.3f)] private float lowWidthScale = 1.08f;
        [SerializeField, Min(0f)] private float lowLegSpread = 0.1f;
        [SerializeField] private float highHeightOffset = 0.22f;
        [SerializeField, Range(1f, 1.4f)] private float highVerticalScale = 1.12f;
        [SerializeField, Range(0.75f, 1f)] private float highWidthScale = 0.96f;
        [SerializeField, Min(0f)] private float highLegNarrowing = 0.04f;

        private Vector3 torsoPosition;
        private Vector3 headPosition;
        private Vector3 leftLegPosition;
        private Vector3 rightLegPosition;
        private Vector3 leftArmPosition;
        private Vector3 rightArmPosition;
        private Vector3 stripePosition;
        private Vector3 torsoScale;
        private Vector3 leftLegScale;
        private Vector3 rightLegScale;
        private Vector3 leftArmScale;
        private Vector3 rightArmScale;
        private Vector3 stripeScale;
        private bool neutralPoseCaptured;
        private bool hasAppliedStance;
        private PlayerStance appliedStance = PlayerStance.Medium;

        public PoundDribbleController StanceSource => stanceSource;
        public PlayerStance AppliedStance => appliedStance;
        public float HeadLocalHeight => head ? head.localPosition.y : 0f;
        public float NeutralHeadLocalHeight => headPosition.y;

        private void Awake()
        {
            CaptureNeutralPose();
            RefreshImmediate();
        }

        private void LateUpdate()
        {
            if (stanceSource != null && (!hasAppliedStance || appliedStance != stanceSource.CurrentStance))
                ApplyStance(stanceSource.CurrentStance);
        }

        public void Configure(PoundDribbleController source, Transform torsoPart, Transform headPart,
            Transform leftLegPart, Transform rightLegPart, Transform leftArmPart, Transform rightArmPart,
            Transform stripePart)
        {
            stanceSource = source;
            torso = torsoPart;
            head = headPart;
            leftLeg = leftLegPart;
            rightLeg = rightLegPart;
            leftArm = leftArmPart;
            rightArm = rightArmPart;
            jerseyStripe = stripePart;
            neutralPoseCaptured = false;
            hasAppliedStance = false;
            CaptureNeutralPose();
            RefreshImmediate();
        }

        public void RefreshImmediate()
        {
            if (stanceSource == null) return;
            if (!neutralPoseCaptured) CaptureNeutralPose();
            ApplyStance(stanceSource.CurrentStance);
        }

        private void CaptureNeutralPose()
        {
            if (!HasRequiredParts()) return;
            torsoPosition = torso.localPosition;
            headPosition = head.localPosition;
            leftLegPosition = leftLeg.localPosition;
            rightLegPosition = rightLeg.localPosition;
            leftArmPosition = leftArm.localPosition;
            rightArmPosition = rightArm.localPosition;
            stripePosition = jerseyStripe.localPosition;
            torsoScale = torso.localScale;
            leftLegScale = leftLeg.localScale;
            rightLegScale = rightLeg.localScale;
            leftArmScale = leftArm.localScale;
            rightArmScale = rightArm.localScale;
            stripeScale = jerseyStripe.localScale;
            neutralPoseCaptured = true;
        }

        private void ApplyStance(PlayerStance stance)
        {
            if (!neutralPoseCaptured || !HasRequiredParts()) return;
            RestoreNeutralPose();

            if (stance == PlayerStance.Low)
                ApplyPose(lowHeightOffset, lowVerticalScale, lowWidthScale, lowLegSpread);
            else if (stance == PlayerStance.High)
                ApplyPose(highHeightOffset, highVerticalScale, highWidthScale, -highLegNarrowing);

            appliedStance = stance;
            hasAppliedStance = true;
        }

        private void ApplyPose(float heightOffset, float verticalScale, float widthScale, float legSpread)
        {
            torso.localPosition = torsoPosition + Vector3.up * (heightOffset * 0.55f);
            head.localPosition = headPosition + Vector3.up * heightOffset;
            leftLeg.localPosition = leftLegPosition + Vector3.up * (heightOffset * 0.18f) + Vector3.left * legSpread;
            rightLeg.localPosition = rightLegPosition + Vector3.up * (heightOffset * 0.18f) + Vector3.right * legSpread;
            leftArm.localPosition = leftArmPosition + Vector3.up * (heightOffset * 0.65f);
            rightArm.localPosition = rightArmPosition + Vector3.up * (heightOffset * 0.65f);
            jerseyStripe.localPosition = stripePosition + Vector3.up * (heightOffset * 0.55f);

            torso.localScale = ScalePose(torsoScale, verticalScale, widthScale);
            leftLeg.localScale = ScalePose(leftLegScale, verticalScale, 1f);
            rightLeg.localScale = ScalePose(rightLegScale, verticalScale, 1f);
            leftArm.localScale = ScalePose(leftArmScale, verticalScale, 1f);
            rightArm.localScale = ScalePose(rightArmScale, verticalScale, 1f);
            jerseyStripe.localScale = ScalePose(stripeScale, verticalScale, widthScale);
        }

        private void RestoreNeutralPose()
        {
            torso.localPosition = torsoPosition;
            head.localPosition = headPosition;
            leftLeg.localPosition = leftLegPosition;
            rightLeg.localPosition = rightLegPosition;
            leftArm.localPosition = leftArmPosition;
            rightArm.localPosition = rightArmPosition;
            jerseyStripe.localPosition = stripePosition;
            torso.localScale = torsoScale;
            leftLeg.localScale = leftLegScale;
            rightLeg.localScale = rightLegScale;
            leftArm.localScale = leftArmScale;
            rightArm.localScale = rightArmScale;
            jerseyStripe.localScale = stripeScale;
        }

        private bool HasRequiredParts()
        {
            return torso && head && leftLeg && rightLeg && leftArm && rightArm && jerseyStripe;
        }

        private static Vector3 ScalePose(Vector3 baseline, float verticalScale, float widthScale)
        {
            return new Vector3(baseline.x * widthScale, baseline.y * verticalScale, baseline.z * widthScale);
        }
    }
}
