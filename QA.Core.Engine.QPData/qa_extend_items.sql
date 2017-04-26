-- =============================================
-- Author:		KarlovN
-- Create date:
-- Description:	Загрузка расширений. Предполагается, что поле-ссылка на основную статью называется ItemId
--				В зависимости от значения @includeBaseFields догружаются также поля основной статьи
-- =============================================
if exists(SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.qa_extend_items'))
BEGIN
	DROP PROCEDURE [dbo].[qa_extend_items]
END
GO

CREATE PROCEDURE [dbo].[qa_extend_items]
	@ContentId int, -- ID контента расширения
	@IsLive bit = 1,
	@Ids ListOfIds readonly,
	@includeBaseFields bit = 0, -- догружать ли поля основной статьи
	@baseContentId int = 0 -- ID основного контента (AbstractItem)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @isSplitted AS BIT = NULL

	DECLARE @tablename NVARCHAR(255) = N'CONTENT_' + CAST(@contentId AS NVARCHAR(255))
	DECLARE @tablesuffix NVARCHAR(255) = N''

	DECLARE @abstractItemViewName NVARCHAR(255) = N'CONTENT_'

	IF (@isSplitted IS NULL)
	BEGIN
		SELECT TOP 1 @isSplitted = CONTENT.USE_DEFAULT_FILTRATION
			FROM dbo.[CONTENT]
				WHERE CONTENT.CONTENT_ID = @contentid
	END

	IF (@isSplitted = 1)
	BEGIN
		IF (@isLive = 1)
			SET @tablesuffix = '_LIVE'
		ELSE
			SET @tablesuffix = '_STAGE'
	END

	SET @tablename = @tablename + @tablesuffix

	SET @tablesuffix = ''
	IF(@includeBaseFields = 1 AND @baseContentId > 0)
	BEGIN
		SELECT TOP 1 @isSplitted = CONTENT.USE_DEFAULT_FILTRATION
			FROM dbo.[CONTENT]
				WHERE CONTENT.CONTENT_ID = @baseContentId

		IF (@isSplitted = 1)
		BEGIN
			IF (@isLive = 1)
				SET @tablesuffix = '_LIVE'
			ELSE
				SET @tablesuffix = '_STAGE'
		END

		SET @abstractItemViewName = @abstractItemViewName + CAST(@baseContentId AS NVARCHAR(255)) + @tablesuffix
	END


	DECLARE @query NVARCHAR(MAX)

	IF (@ContentId > 0)
	BEGIN
		SET @query = N'
			SELECT
			*
			FROM ' + @tablename + ' ext with (nolock)
			INNER JOIN @x on Id = ext.itemid
			'

		IF(@includeBaseFields = 1 AND @baseContentId > 0)
		BEGIN
			SET @query = @query + N' INNER JOIN ' + @abstractItemViewName + ' ai on ai.Content_item_id = ext.itemid'
		END
	END
	ELSE IF(@includeBaseFields = 1 AND @baseContentId > 0)
	BEGIN
		SET @query = N'
			SELECT
			*
			FROM ' + @abstractItemViewName + ' ai with (nolock)
			INNER JOIN @x on Id = ai.Content_item_id
			'
	END
	ELSE
	BEGIN
		-- по-хорошему сюда попадать не должно, т.к. если @includeBaseFields=0, то @ContentId должен быть положительным
		-- подстрахуюсь на всякий случай. Вернем просто ID
		SET @query = N'
			SELECT Id as Content_item_id FROM @x
			'
	END

	print @query

	exec dbo.SP_EXECUTESQL @query, N'@x ListOfIds READONLY', @x = @ids;

END
GO
