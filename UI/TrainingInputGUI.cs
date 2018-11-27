using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingInputGUI : MonoBehaviour {

    private List<List<Image>> player1ButtonPresses = new List<List<Image>>(8);
    private List<List<Image>> player2ButtonPresses = new List<List<Image>>(8);

    public void OnVeiwingButtonIcon(ButtonReference[] buttonReferences, int player)
    {
        if (buttonReferences != null && buttonReferences.Length > 0)
        {
            List<Sprite> activeIconList = new List<Sprite>();
            foreach (ButtonReference btnRef in buttonReferences)
            {
                if (btnRef != null && btnRef.activeIconImg != null)
                {
                    Sprite sprite = Sprite.Create(btnRef.activeIconImg,
                                                  new Rect(0f, 0f, btnRef.activeIconImg.width, btnRef.activeIconImg.height),
                                                  new Vector2(0.5f * btnRef.activeIconImg.width, 0.5f * btnRef.activeIconImg.height));
                    sprite.name = btnRef.activeIconImg.name;
                    activeIconList.Add(sprite);
                }
            }

            List<List<Image>> playerButtonPresses = null;
            if (player == 1)
            {
                playerButtonPresses = this.player1ButtonPresses;
            }
            else if (player == 2)
            {
                playerButtonPresses = this.player2ButtonPresses;
            }

            if (activeIconList.Count > 0)
            {
                List<Image> images = new List<Image>();
                foreach (Sprite sprite in activeIconList)
                {
                    GameObject iconObj = new GameObject(sprite.name);

                    iconObj.transform.parent = null;
                    iconObj.transform.localPosition = Vector3.zero;
                    iconObj.transform.localRotation = Quaternion.identity;
                    iconObj.transform.localScale = Vector3.one;
                    iconObj.transform.parent = this.transform;

                    Image image = iconObj.AddComponent<Image>();
                    image.sprite = sprite;
                    image.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    images.Add(image);
                }
                playerButtonPresses.Add(images);
            }

            while (playerButtonPresses.Count >= 10)
            {
                foreach (Image image in playerButtonPresses[0])
                {
                    if (image != null)
                    {
                        GameObject.Destroy(image.gameObject);
                    }
                }
                playerButtonPresses.RemoveAt(0);
            }

            for (int i = 0; i < playerButtonPresses.Count; i++)
            {
                float distance = 0;
                foreach (Image image in playerButtonPresses[i])
                {
                    if (image != null && image.rectTransform)
                    {
                        float x = player == 1 ? 0f : 1f;
                        float y = Mathf.Lerp(0.78f, 0.1f, (float)i / 9f);

                        image.rectTransform.anchorMin = new Vector2(x, y);
                        image.rectTransform.anchorMax = image.rectTransform.anchorMin;
                        image.rectTransform.anchoredPosition = Vector2.zero;
                        image.rectTransform.offsetMax = Vector2.zero;
                        image.rectTransform.offsetMin = Vector2.zero;
                        image.rectTransform.sizeDelta = new Vector2(image.preferredWidth * 1.5f, image.preferredHeight * 1.5f);

                        if (player == 1)
                        {
                            image.rectTransform.pivot = new Vector2(0f, 0.5f);
                            image.rectTransform.anchoredPosition = new Vector2(image.rectTransform.sizeDelta.x * distance, 0);
                        }
                        else
                        {
                            image.rectTransform.pivot = new Vector2(1f, 0.5f);
                            image.rectTransform.anchoredPosition = new Vector2(-image.rectTransform.sizeDelta.x * distance, 0);
                        }
                        distance += 0.6f;
                    }
                }
            }
        }
    }
}
