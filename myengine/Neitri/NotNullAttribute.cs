using System;

namespace Neitri
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
	public class NotNullAttribute : Attribute
	{
	}
}