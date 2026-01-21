BEGIN TRANSACTION;
ALTER TABLE [OrganizationMember] ADD [IsAccepted] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260121103316_AddOrganizationMemberIsAccepted', N'10.0.1');

COMMIT;
GO

