void GrassVertexManipulation(float3 VertexPosition, float3 PlayerPosition, float Intensity, out float3 ModifiedPosition)
{
    // Get the vertex height (in object space)
    float vertexHeight = VertexPosition.y;
    
    // Calculate direction from vertex to player (in object space)
    float3 toPlayer = normalize(PlayerPosition - VertexPosition);
    
    // The higher the vertex, the more it should bend
    float heightFactor = saturate(vertexHeight / 1.0); // Adjust the 1.0 to match your grass height
    
    // Calculate the bend direction (opposite to player direction)
    float3 bendDirection = -toPlayer;
    bendDirection.y = 0; // Keep the bend horizontal
    
    // Calculate final displacement
    float3 displacement = bendDirection * Intensity * heightFactor;
    
    // Apply the displacement to the vertex
    ModifiedPosition = VertexPosition + displacement;
}