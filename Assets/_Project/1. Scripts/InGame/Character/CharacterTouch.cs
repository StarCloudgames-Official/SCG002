using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterTouch : TouchableObject
{
    [SerializeField] private Transform attackRangeTransform;

    private Camera mainCamera;
    private CharacterBehaviour characterBehaviour;
    private CharacterGridManager gridManager;
    
    public void Initialize(CharacterBehaviour characterBehaviour)
    {
        this.characterBehaviour = characterBehaviour;

        mainCamera = ObjectRegister.TryGet<Camera>(ObjectRegister.RegisterType.MainCamera, out var camera) ? camera : Camera.main;
        gridManager = InGameManager.Instance.InGameContext.CharacterGridManager;
    }

    private void ShowAttackRange()
    {
        //TODO : 스탯매니저에서 공격범위 증가 뭐시기 받아와야됨
        var attackRange = characterBehaviour.CurrentClass.attackRange;
        // attackRange는 반지름이므로 스프라이트 지름은 2배로 설정
        attackRangeTransform.localScale = Vector3.one * attackRange * 2f;
        attackRangeTransform.gameObject.SetActive(true);
    }

    private void DisableAttackRange()
    {
        attackRangeTransform.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        characterBehaviour.CanInteract = false;
        
        ShowAttackRange();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        DisableAttackRange();
        
        characterBehaviour.CanInteract = true;
    }

    public override void OnDragEnd(PointerEventData eventData)
    {
        var worldPosition = VectorExtensions.EventPointerToVector2(eventData, mainCamera);
        var targetGrid = gridManager.GetCharacterGridFromWorldPosition(worldPosition);
        var currentGrid = characterBehaviour.CurrentGrid;

        if (targetGrid == null || targetGrid == currentGrid)
        {
            if (currentGrid != null)
            {
                CachedTransform.position = currentGrid.transform.position;
            }
            return;
        }

        if (targetGrid.IsEmpty)
        {
            currentGrid?.Clear();
            characterBehaviour.SetToGrid(targetGrid);
        }
        else
        {
            var targetCharacter = targetGrid.CurrentBehaviour;
            currentGrid?.Clear();
            targetGrid.Clear();

            characterBehaviour.SetToGrid(targetGrid);
            targetCharacter.SetToGrid(currentGrid);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        CachedTransform.transform.position = VectorExtensions.EventPointerToVector2(eventData, mainCamera);
    }
}