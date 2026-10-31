# Microsoft canonical migration — QA runner

Administrative QA runner for010-A; no HTTP linking API, passwords, provider changes or EF auto-migration.
It changes only the existing Users canonical identity and migration journal in a disposable QA database.
Domain UserId/FKs never change. Production names are rejected; production cutover requires a later approved runbook.

1. Prepare schema and pause new provisioning for the QA pilot. Keep Microsoft legacy available.
2. Private JSON outside Git: Environment=QA, AllowedQaDatabase (prefix MiKompri010QA or users010a_),
   SourceTenantId, TargetTenantId, FederationIssuer verified against actual CIAM federation,
   UserId of the existing QA Microsoft row, TargetObjectId and ApprovalDigest (SHA256 of cohort authorization).
3. Supply MIKOMPRI_MIGRATION_QA_DATABASE and MIKOMPRI_MIGRATION_GRAPH_TOKEN only through protected local environment.
   Graph token must read organization/user identities in target QA; no new grants are obtained by this runner.
4. Run dotnet run --project tools/identity-migration -- reserve <private-config.json>.
   Runner verifies target tenant and exact immutable source object in its federated identities.
   It rejects occupied destination, mismatched source, missing proof and suspended source.
5. Record returned migration Id/version in the private config. Apply uses expectedVersion0 and rechecks Graph origin.
6. Verify login and manifest of UserId/owner/memberships/lists/expenses/purchases/history.
7. Rollback uses expectedVersion1 and restores only approved source; preserves profile, new purchases, suspension and renewed token cuts.
   Reacquire a token issued strictly after the cut (same second is denied). No Down/ownership rewrite.

Reservations and provisioning share PostgreSQL transaction advisory locks and unique indexes.
Retired/reserved tuples cannot auto-provision even through legacy sub fallback.
No email/profile matching; no alternative Google identity to repair Microsoft migration.
Evidence must contain digests/results, no configuration, personal email, Graph token or database credentials.
