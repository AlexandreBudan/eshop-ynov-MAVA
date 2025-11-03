# Tests Unitaires - Discount Calculator

## Vue d'ensemble

Cette suite de tests valide toutes les règles métier du système de calcul de réductions, incluant les coupons simples, cumulés, expirés, et les règles contextuelles avancées.

## Exécution des tests

```bash
dotnet test Discount.Grpc.Tests.csproj
```

## Résultats des tests

✅ **19/19 tests passent avec succès**

## Couverture des scénarios

### 1. Coupon Seul (Single Coupon Tests)

#### ✅ Test: Coupon avec pourcentage
- **Scénario**: Appliquer un coupon de 10% sur un prix de 100€
- **Résultat attendu**: Réduction de 10€, prix final de 90€
- **Statut**: ✅ PASS

#### ✅ Test: Coupon avec montant fixe
- **Scénario**: Appliquer un coupon de 20€ fixe sur un prix de 100€
- **Résultat attendu**: Réduction de 20€, prix final de 80€
- **Statut**: ✅ PASS

#### ✅ Test: Coupon combiné (pourcentage + fixe)
- **Scénario**: Appliquer un coupon de 10% + 5€ sur un prix de 100€
- **Résultat attendu**: Réduction de 15€ (10€ + 5€), prix final de 85€
- **Statut**: ✅ PASS

### 2. Réductions Cumulées (Stacked Discounts Tests)

#### ✅ Test: Coupons cumulables avec priorités
- **Scénario**: Deux coupons cumulables (10% priorité 2, 15% priorité 1) sur 100€
- **Calcul**:
  1. Priorité 2 appliquée en premier: 100€ × 10% = 10€, reste 90€
  2. Priorité 1 ensuite: 90€ × 15% = 13.5€, reste 76.5€
- **Résultat attendu**: Réduction totale de 23.5€, prix final de 76.5€
- **Statut**: ✅ PASS

#### ✅ Test: Cumul pourcentage + fixe
- **Scénario**: Coupon pourcentage (20%) + coupon fixe (10€) sur 100€
- **Calcul**:
  1. Pourcentage appliqué en premier: 100€ × 20% = 20€, reste 80€
  2. Fixe ensuite: 80€ - 10€ = 70€
- **Résultat attendu**: Réduction totale de 30€, prix final de 70€
- **Statut**: ✅ PASS

#### ✅ Test: Dépassement du maximum de cumul (30%)
- **Scénario**: Deux coupons de 20% et 15% (total 35% > max 30%)
- **Résultat attendu**: Limité à 30% maximum avec avertissement
- **Statut**: ✅ PASS

#### ✅ Test: Coupon non-cumulable
- **Scénario**: Un coupon non-cumulable (50%) avec un autre coupon (10%)
- **Résultat attendu**: Seulement le coupon non-cumulable est appliqué (50€), avec message d'avertissement
- **Statut**: ✅ PASS

### 3. Coupons Expirés/Invalides (Invalid/Expired Coupon Tests)

#### ✅ Test: Coupon expiré
- **Scénario**: Coupon avec status "Expired" et dates passées
- **Résultat attendu**: Aucune réduction appliquée
- **Statut**: ✅ PASS

#### ✅ Test: Coupon désactivé
- **Scénario**: Coupon avec status "Disabled"
- **Résultat attendu**: Aucune réduction appliquée
- **Statut**: ✅ PASS

#### ✅ Test: Limite d'utilisation atteinte
- **Scénario**: Coupon avec MaxUsageCount = CurrentUsageCount
- **Résultat attendu**: Aucune réduction appliquée
- **Statut**: ✅ PASS

#### ✅ Test: Montant minimum non atteint
- **Scénario**: Prix de 50€ avec coupon nécessitant minimum 100€
- **Résultat attendu**: Aucune réduction appliquée
- **Statut**: ✅ PASS

### 4. Règles Contextuelles (Contextual Rules Tests)

#### ✅ Test: Réduction par catégorie - Catégorie correspondante
- **Scénario**: Coupon pour "Electronics" appliqué à un produit "Electronics"
- **Résultat attendu**: Réduction de 10€ appliquée
- **Statut**: ✅ PASS

#### ✅ Test: Réduction par catégorie - Catégorie non correspondante
- **Scénario**: Coupon pour "Electronics" appliqué à un produit "Books"
- **Résultat attendu**: Aucune réduction appliquée
- **Statut**: ✅ PASS

#### ✅ Test: Réduction globale
- **Scénario**: Coupon avec scope "Global" appliqué à n'importe quel produit
- **Résultat attendu**: Réduction appliquée quelle que soit la catégorie
- **Statut**: ✅ PASS

#### ✅ Test: Réduction par paliers (Tiered Discount)
- **Scénario**: Coupon avec paliers (5% < 100€, 10% < 200€, 15% ≥ 200€) sur 250€
- **Résultat attendu**: Palier de 15% appliqué = 37.5€ de réduction
- **Statut**: ✅ PASS

#### ✅ Test: Campagne automatique
- **Scénario**: Coupon "Black Friday" automatique (sans code requis)
- **Résultat attendu**: Réduction de 20% appliquée automatiquement
- **Statut**: ✅ PASS

### 5. Cas Limites (Edge Cases)

#### ✅ Test: Prix zéro
- **Scénario**: Appliquer une réduction sur un prix de 0€
- **Résultat attendu**: Aucune réduction, message d'avertissement
- **Statut**: ✅ PASS

#### ✅ Test: Aucun coupon
- **Scénario**: Calculer avec une liste vide de coupons
- **Résultat attendu**: Prix original inchangé
- **Statut**: ✅ PASS

#### ✅ Test: Réduction dépassant le prix
- **Scénario**: Coupon de 100€ sur un prix de 50€
- **Résultat attendu**: Réduction plafonnée à 50€ (prix final = 0€), avec avertissement
- **Statut**: ✅ PASS

## Règles Métier Validées

### Protection contre les abus
1. ✅ Prix plancher à 0€ (pas de prix négatif)
2. ✅ Limite de cumul de pourcentages (30% par défaut)
3. ✅ Réductions non-cumulables prioritaires
4. ✅ Plafonnement automatique si réduction > prix

### Ordre d'application
1. ✅ Coupons non-cumulables en premier
2. ✅ Puis par priorité (plus haute en premier)
3. ✅ Puis par type (pourcentage avant fixe)

### Validation des coupons
1. ✅ Status doit être "Active"
2. ✅ Dates de validité (StartDate/EndDate)
3. ✅ Limites d'utilisation (MaxUsageCount)
4. ✅ Montant minimum d'achat (MinimumPurchaseAmount)

### Règles contextuelles
1. ✅ Scopes: Product, Category, Cart, Global
2. ✅ Réductions par paliers selon le montant
3. ✅ Campagnes automatiques sans code
4. ✅ Filtrage par catégories de produits

## Architecture des Tests

```
DiscountCalculatorTests.cs
├── Single Coupon Tests (3 tests)
├── Stacked Discounts Tests (4 tests)
├── Non-Stackable Coupon Tests (1 test)
├── Invalid/Expired Coupon Tests (4 tests)
├── Contextual Rules Tests (5 tests)
└── Edge Cases (3 tests)
```

## Technologies Utilisées

- **Framework de test**: xUnit
- **Assertions**: FluentAssertions
- **Mocking**: Moq (prêt mais non utilisé pour ces tests unitaires purs)

## Prochaines Étapes

- [ ] Tests d'intégration pour Basket.API
- [ ] Tests d'intégration pour Catalog.API
- [ ] Tests end-to-end des scénarios complets
- [ ] Tests de performance pour gros volumes
