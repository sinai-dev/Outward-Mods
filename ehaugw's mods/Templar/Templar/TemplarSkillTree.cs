using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200000E RID: 14
	public class TemplarSkillTree
	{
		// Token: 0x06000019 RID: 25 RVA: 0x000030BC File Offset: 0x000012BC
		public static void SetupSkillTree(ref SkillSchool templarTreeInstance)
		{
			SL_SkillTree sl_SkillTree = new SL_SkillTree
			{
				Name = "Templar",
				SkillRows = new List<SL_SkillRow>
				{
					new SL_SkillRow
					{
						RowIndex = 1,
						Slots = new List<SL_BaseSkillSlot>
						{
							new SL_SkillSlot
							{
								ColumnIndex = 1,
								SilverCost = 50,
								SkillID = 2502000,
								Breakthrough = false,
								RequiredSkillSlot = Vector2.zero
							},
							new SL_SkillSlot
							{
								ColumnIndex = 3,
								SilverCost = 50,
								SkillID = 2502026,
								Breakthrough = false,
								RequiredSkillSlot = Vector2.zero
							}
						}
					},
					new SL_SkillRow
					{
						RowIndex = 2,
						Slots = new List<SL_BaseSkillSlot>()
					},
					new SL_SkillRow
					{
						RowIndex = 3,
						Slots = new List<SL_BaseSkillSlot>
						{
							new SL_SkillSlot
							{
								ColumnIndex = 2,
								SilverCost = 500,
								SkillID = 2502002,
								Breakthrough = true,
								RequiredSkillSlot = Vector2.zero
							}
						}
					},
					new SL_SkillRow
					{
						RowIndex = 4,
						Slots = new List<SL_BaseSkillSlot>
						{
							new SL_SkillSlot
							{
								ColumnIndex = 2,
								SilverCost = 600,
								SkillID = 2502001,
								Breakthrough = false,
								RequiredSkillSlot = new Vector2(3f, 2f)
							},
							new SL_SkillSlot
							{
								ColumnIndex = 3,
								SilverCost = 600,
								SkillID = 2502009,
								Breakthrough = false,
								RequiredSkillSlot = new Vector2(3f, 2f)
							},
							new SL_SkillSlot
							{
								ColumnIndex = 1,
								SilverCost = 600,
								SkillID = 2502010,
								Breakthrough = false,
								RequiredSkillSlot = new Vector2(3f, 2f)
							}
						}
					},
					new SL_SkillRow
					{
						RowIndex = 5,
						Slots = new List<SL_BaseSkillSlot>
						{
							new SL_SkillSlotFork
							{
								ColumnIndex = 2,
								RequiredSkillSlot = new Vector2(4f, 2f),
								Choice1 = new SL_SkillSlot
								{
									ColumnIndex = 2,
									SilverCost = 600,
									SkillID = 2502006,
									RequiredSkillSlot = new Vector2(4f, 2f)
								},
								Choice2 = new SL_SkillSlot
								{
									ColumnIndex = 2,
									SilverCost = 600,
									SkillID = 2502008,
									RequiredSkillSlot = new Vector2(4f, 2f)
								}
							}
						}
					}
				}
			};
			templarTreeInstance = sl_SkillTree.CreateBaseSchool();
			sl_SkillTree.ApplyRows();
		}
	}
}
