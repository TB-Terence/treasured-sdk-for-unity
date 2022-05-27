using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Treasured.UnitySdk.Interaction
{
	public sealed class EndNode : Node
	{
		[Input(ShowBackingValue.Never)]
		public Node previous;

		// Use this for initialization
		protected override void Init()
		{
			base.Init();

		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}
	}
}