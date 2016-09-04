    using UnityEngine;
    using System.Collections;
    using System;
    using System.Linq;


namespace Neitri
{
	    static class AnimatorExtensions
	    {
	        public static void SetLayerWeight(this Animator animator, string layerName, float layerWeight)
	        {
	            for (int i = 0; i < animator.layerCount; i++)
	            {
	                if (animator.GetLayerName(i) == layerName)
	                {
	                    animator.SetLayerWeight(i, layerWeight);
	                    return;
	                }
	            }
	
	        }
	
	        public static void LerpIKPosition(this Animator animator, AvatarIKGoal goal, Vector3 value, float time)
	        {
	            animator.SetIKPosition(goal, Vector3.Lerp(animator.GetIKPosition(goal), value, time));
	        }
	        public static void LerpIKPositionWeight(this Animator animator, AvatarIKGoal goal, float value, float time)
	        {
	            animator.SetIKPositionWeight(goal, Mathf.Lerp(animator.GetIKPositionWeight(goal), value, time));
	        }
	        public static void LerpIKRotation(this Animator animator, AvatarIKGoal goal, Quaternion value, float time)
	        {
	            animator.SetIKRotation(goal, Quaternion.Lerp(animator.GetIKRotation(goal), value, time));
	        }
	        public static void LerpIKRotationWeight(this Animator animator, AvatarIKGoal goal, float value, float time)
	        {
	            animator.SetIKRotationWeight(goal, Mathf.Lerp(animator.GetIKRotationWeight(goal), value, time));
	        }
	
	    }
	
}