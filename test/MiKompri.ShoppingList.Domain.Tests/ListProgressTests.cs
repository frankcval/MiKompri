// Plan (pseudocódigo detallado):
// 1. Importar namespaces necesarios: Xunit y el namespace del ValueObject.
// 2. Crear clase de pruebas `ListProgressTests`.
// 3. Incluir pruebas unitarias:
//    - `Percentage_WhenTotalIsZero_ReturnsZero`:
//        Crear ListProgress con total=0, purchased cualquier (0). Comprobar que Percentage == 0.
//    - `Percentage_RoundsToTwoDecimals`:
//        Crear ListProgress con valores que produzcan una fracción repetida (ej. 1/3).
//        Comprobar que Percentage == 33.33 (redondeado a 2 decimales).
//    - `Create_ReturnsInstanceWithExpectedPercentage`:
//        Usar factory `Create` y comprobar Percentage calculado.
//    - `Equality_SamePercentage_AreEqual`:
//        Crear dos ListProgress distintos en componentes pero con mismo porcentaje (ej. 1/2 y 2/4).
//        Comprobar que Equals devuelve true y que GetHashCode coincide (comportamiento actual del ValueObject).
//    - `Equality_DifferentPercentage_NotEqual`:
//        Crear dos ListProgress con porcentajes distintos y comprobar que no son iguales.
// 4. Usar Assert.Equal/True/False según corresponda.
// 5. Mantener pruebas pequeñas y deterministas.
//
// Notas:
// - Las pruebas reflejan el comportamiento actual de `ListProgress`, que compara componentes de igualdad
//   basándose en `Percentage` (según GetEqualityComponents). Por tanto dos instancias con distinto
//   total/purchased pero mismo porcentaje serán consideradas iguales.

using Xunit;
using MiKompri.ShoppingList.Domain.ValueObjects;

namespace MiKompri.ShoppingList.Tests.Domain.ValueObjects
{
    public class ListProgressTests
    {
        [Fact]
        public void Percentage_WhenTotalIsZero_ReturnsZero()
        {
            var lp = ListProgress.Create(0, 0);
            Assert.Equal(0d, lp.Percentage);
        }

        [Fact]
        public void Percentage_RoundsToTwoDecimals()
        {
            // 1 / 3 => 33.333... => 33.33 después de redondear a 2 decimales
            var lp = ListProgress.Create(3, 1);
            Assert.Equal(33.33d, lp.Percentage);
        }

        [Fact]
        public void Create_ReturnsInstanceWithExpectedPercentage()
        {
            var lp = ListProgress.Create(4, 1); // 25.00%
            Assert.Equal(25.00d, lp.Percentage);
        }

        [Fact]
        public void Equality_SamePercentage_AreEqual()
        {
            // 1/2 => 50.00, 2/4 => 50.00. Según GetEqualityComponents solo se considera Percentage.
            var a = ListProgress.Create(2, 1);
            var b = ListProgress.Create(4, 2);

            Assert.Equal(a, b); // utiliza Equals
            Assert.True(a.Equals(b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_DifferentPercentage_NotEqual()
        {
            var a = ListProgress.Create(5, 2); // 40.00
            var b = ListProgress.Create(4, 2); // 50.00

            Assert.NotEqual(a, b);
            Assert.False(a.Equals(b));
        }
    }
}