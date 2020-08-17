using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000022 RID: 34
	public class JuggernautSkillTree
	{
		// Token: 0x06000066 RID: 102 RVA: 0x00004698 File Offset: 0x00002898
		public static void SetupSkillTree(ref SkillSchool juggernautTreeInstance)
		{
			SL_SkillTree sl_SkillTree = new SL_SkillTree
			{
				Name = "Juggernaut",
				SkillRows = new List<SL_SkillRow>
				{
					new SL_SkillRow
					{
						RowIndex = 1,
						Slots = new List<SL_BaseSkillSlot>
						{
							new SL_SkillSlotFork
							{
								ColumnIndex = 2,
								RequiredSkillSlot = Vector2.zero,
								Choice1 = new SL_SkillSlot
								{
									ColumnIndex = 2,
									SilverCost = 50,
									SkillID = 2502020,
									RequiredSkillSlot = Vector2.zero,
									Breakthrough = false
								},
								Choice2 = new SL_SkillSlot
								{
									ColumnIndex = 2,
									SilverCost = 50,
									SkillID = 2502019,
									RequiredSkillSlot = Vector2.zero,
									Breakthrough = false
								}
							}
						}
					},
					new SL_SkillRow
					{
						RowIndex = 2,
						Slots = new List<SL_BaseSkillSlot>
						{
							new SL_SkillSlot
							{
								ColumnIndex = 1,
								SilverCost = 100,
								SkillID = 2502017,
								Breakthrough = false,
								RequiredSkillSlot = Vector2.zero
							}
						}
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
								SkillID = 2502022,
								Breakthrough = true,
								RequiredSkillSlot = new Vector2(1f, 2f)
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
								ColumnIndex = 1,
								SilverCost = 600,
								SkillID = 2502021,
								Breakthrough = false,
								RequiredSkillSlot = new Vector2(3f, 2f)
							},
							new SL_SkillSlot
							{
								ColumnIndex = 3,
								SilverCost = 600,
								SkillID = 2502015,
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
								RequiredSkillSlot = new Vector2(3f, 2f),
								Choice1 = new SL_SkillSlot
								{
									ColumnIndex = 2,
									SilverCost = 600,
									SkillID = 2502025,
									RequiredSkillSlot = new Vector2(3f, 2f),
									Breakthrough = false
								},
								Choice2 = new SL_SkillSlot
								{
									ColumnIndex = 2,
									SilverCost = 600,
									SkillID = 2502024,
									RequiredSkillSlot = new Vector2(3f, 2f),
									Breakthrough = false
								}
							}
						}
					}
				}
			};
			juggernautTreeInstance = sl_SkillTree.CreateBaseSchool();
			sl_SkillTree.ApplyRows();
		}
	}
}
