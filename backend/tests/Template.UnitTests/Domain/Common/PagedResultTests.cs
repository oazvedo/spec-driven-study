using AwesomeAssertions;
using Template.Domain.Common;

namespace Template.UnitTests.Domain.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(25, 20, 2)]
    [InlineData(40, 20, 2)]
    [InlineData(41, 20, 3)]
    [InlineData(0, 20, 0)]
    public void TotalPages_ArredondaParaCima(int totalCount, int pageSize, int esperado)
    {
        var resultado = new PagedResult<int>([], 1, pageSize, totalCount);

        resultado.TotalPages.Should().Be(esperado);
    }

    [Fact]
    public void TotalPages_PageSizeZero_RetornaZeroSemDividirPorZero()
    {
        var resultado = new PagedResult<int>([], 1, 0, 25);

        resultado.TotalPages.Should().Be(0);
        resultado.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PrimeiraPagina_TemProximaENaoTemAnterior()
    {
        var resultado = new PagedResult<int>([], 1, 20, 25);

        resultado.HasNextPage.Should().BeTrue();
        resultado.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void UltimaPagina_NaoTemProximaETemAnterior()
    {
        var resultado = new PagedResult<int>([], 2, 20, 25);

        resultado.HasNextPage.Should().BeFalse();
        resultado.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void Map_ConverteItensEPreservaPaginacao()
    {
        var resultado = new PagedResult<int>([1, 2, 3], 2, 3, 9);

        var mapeado = resultado.Map(x => $"item {x}");

        mapeado.Items.Should().Equal("item 1", "item 2", "item 3");
        mapeado.Should().BeEquivalentTo(new { Page = 2, PageSize = 3, TotalCount = 9 });
    }
}
