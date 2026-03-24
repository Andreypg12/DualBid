CREATE TRIGGER trg_Auction_UpdateEndDate
ON Auction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Estados que indican que la subasta ha terminado
    -- 3: Finalizada normalmente
    -- 4: Cancelada
    DECLARE @FinalStates TABLE (state_id INT);
    INSERT INTO @FinalStates VALUES (3), (4);
    
    -- Actualizar actual_end_date cuando se cambia a un estado final
    UPDATE a
    SET a.actual_end_date = GETDATE()
    FROM Auction a
    INNER JOIN inserted i ON a.id = i.id
    INNER JOIN deleted d ON a.id = d.id
    WHERE i.state_id IN (SELECT state_id FROM @FinalStates)
      AND d.state_id NOT IN (SELECT state_id FROM @FinalStates)
      AND a.actual_end_date IS NULL;  -- Solo si no tiene fecha
    
    -- si el estado cambia a 4 (Cancelado) -> Comic vuelve a estar disponible (1)
    UPDATE c
    SET c.availability = 1
    FROM Comic c
    INNER JOIN Auction a ON c.id = a.comic_id
    INNER JOIN inserted i ON a.id = i.id
    INNER JOIN deleted d ON a.id = d.id
    WHERE i.state_id = 4                    -- Nuevo estado es Cancelado
      AND d.state_id != 4                   -- Estado anterior NO era Cancelado
      AND c.availability = 0;               -- Solo si está no disponible actualmente
END;