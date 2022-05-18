using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovementUI : MonoBehaviour
{
    public TMP_Text movementText;
    public LineRenderer lineRenderer;

    private CharacterInstanceData _characterToTrack;
    private Vector3 _startingPosition;

    private const float _lineOffsetFromTerrain = 0.2f;

    private Color _defaultColor = new Color(1, 0.43f, 0, 1);
    private Color _invalidColor = new Color(1, 0, 0, 1);


    void Start()
    {
        movementText.gameObject.SetActive(false);
        lineRenderer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (movementText.gameObject.activeInHierarchy)
        {
            movementText.gameObject.transform.position = Input.mousePosition;

            UpdateLine();

            UpdateMovementText();
        }
    }

    private void UpdateLine()
    {
        Vector3 offset = _characterToTrack.transform.position - _startingPosition;
        CheckVisibility(offset);

        int lineSegments = Mathf.RoundToInt(offset.magnitude);

        if (lineSegments == 1)
        {
            lineSegments = 2; // Make sure there's always at least a start and an end
        }

        Vector3[] segmentPositions = new Vector3[lineSegments + 1];

        for (int i = 0; i <= lineSegments; i++)
        {
            float lerpFactor = (float)i / (float)lineSegments;
            Vector3 lerpedPosition = Vector3.Lerp(_startingPosition, _characterToTrack.transform.position, lerpFactor);
            lerpedPosition.y = TerrainManager.Instance.terrainEditor.terrain.SampleHeight(lerpedPosition) + _lineOffsetFromTerrain;
            segmentPositions[i] = lerpedPosition;
        }
        lineRenderer.positionCount = segmentPositions.Length;
        lineRenderer.SetPositions(segmentPositions);
    }

    private void CheckVisibility(Vector3 offset)
    {
        if (offset.magnitude < 1)
        {
            lineRenderer.gameObject.SetActive(false);
            return;
        }

        if (!lineRenderer.gameObject.activeInHierarchy)
        {
            lineRenderer.gameObject.SetActive(true);
        }

        if (GetDistanceInFeet(offset) > _characterToTrack.characterSpeed)
        {
            lineRenderer.material.color = _invalidColor;
        }
        else
        {
            lineRenderer.material.color = _defaultColor;
        }
    }

    private void UpdateMovementText()
    {
        Vector3 offset = _characterToTrack.transform.position - _startingPosition;
        movementText.text = $"{GetDistanceInFeet(offset)} feet";
    }

    private int GetDistanceInFeet(Vector3 offset)
    {
        int xInFeet = Mathf.RoundToInt(Mathf.Abs(offset.x) * 5);
        int zInFeet = Mathf.RoundToInt(Mathf.Abs(offset.z) * 5);
        return Mathf.Max(xInFeet, zInFeet);
    }

    public void ShowMovement(CharacterInstanceData characterToTrack)
    {
        _characterToTrack = characterToTrack;
        _startingPosition = _characterToTrack.transform.position;
        movementText.gameObject.SetActive(true);
        lineRenderer.gameObject.SetActive(true);
        lineRenderer.SetPosition(0, _startingPosition);
        lineRenderer.SetPosition(0, _characterToTrack.transform.position);
    }

    public void HideMovement()
    {
        movementText.gameObject.SetActive(false);
        lineRenderer.gameObject.SetActive(false);
    }
}
