using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteSelector : MonoBehaviour
{
    public Sprite U, L, D, R, UD, LR, DR, DRU, LD, LDR, UL, ULD, ULR, UR, OPEN;

    public bool up, down, left, right;
    public int type; //0: normal, 1: enter
    public Color normalColor, enterColor;
    Color mainColor;
    SpriteRenderer rend;

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        mainColor = normalColor;
        PickSprite();
        PickColor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void PickSprite()
	{ //picks correct sprite based on the four door bools
		if (up)
		{
			if (down)
			{
				if (right)
				{
					if (left)
					{
						rend.sprite = OPEN;
					}
					else
					{
						rend.sprite = DRU;
					}
				}
				else if (left)
				{
					rend.sprite = ULD;
				}
				else
				{
					rend.sprite = UD;
				}
			}
			else
			{
				if (right)
				{
					if (left)
					{
						rend.sprite = ULR;
					}
					else
					{
						rend.sprite = UR;
					}
				}
				else if (left)
				{
					rend.sprite = UL;
				}
				else
				{
					rend.sprite = U;
				}
			}
			return;
		}
		if (down)
		{
			if (right)
			{
				if (left)
				{
					rend.sprite = LDR;
				}
				else
				{
					rend.sprite = DR;
				}
			}
			else if (left)
			{
				rend.sprite = LD;
			}
			else
			{
				rend.sprite = D;
			}
			return;
		}
		if (right)
		{
			if (left)
			{
				rend.sprite = LR;
			}
			else
			{
				rend.sprite = R;
			}
		}
		else
		{
			rend.sprite = L;
		}
	}

	void PickColor()
	{ //changes color based on what type the room is
		if (type == 0)
		{
			mainColor = normalColor;
		}
		else if (type == 1)
		{
			mainColor = enterColor;
		}
		rend.color = mainColor;
	}
}
