using Desafio2APlicacionAPI.Controllers;
using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionAPi.Test
{
    public class OrganizadoresControllerTest
    {
        [Fact]
        public async Task GetOrganizador_RetornaOrganizador_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no hay un valor en caché
            mockDb.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(RedisValue.Null);

            var controller = new OrganizadoresController(context, mockRedis.Object);
            var organizador = new Organizador { Nombre = "Organizador Test", Cargo = "Descripción", EventoId = 1 };
            context.Organizador.Add(organizador);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetOrganizador(organizador.OrganizadorId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Organizador>>(result);
            var returnValue = Assert.IsType<Organizador>(actionResult.Value);
            Assert.Equal("Organizador Test", returnValue.Nombre);
        }
        [Fact]
        public async Task GetOrganizador_RetornaNotFound_CuandoIdNoExiste()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no hay un valor en caché
            mockDb.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(RedisValue.Null);

            var controller = new OrganizadoresController(context, mockRedis.Object);

            // Act
            var result = await controller.GetOrganizador(999);  // ID que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        [Fact]
        public async Task PostOrganizador_IncrementaConteo_CuandoSeAgregaNuevoOrganizador()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            var controller = new OrganizadoresController(context, mockRedis.Object);
            var organizadorInicial = new Organizador { Nombre = "Org1", Cargo = "org1@test.com", EventoId = 1 };

            await controller.PostOrganizador(organizadorInicial);

            var nuevoOrganizador = new Organizador { Nombre = "Org2", Cargo = "org2@test.com", EventoId = 1 };

            // Act
            await controller.PostOrganizador(nuevoOrganizador);
            var organizadores = await context.Organizador.ToListAsync();

            // Assert
            Assert.Equal(2, organizadores.Count);
        }
        [Fact]
        public async Task DeleteOrganizador_EliminaOrganizador_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            var controller = new OrganizadoresController(context, mockRedis.Object);
            var organizador = new Organizador { Nombre = "OrgEliminar", Cargo = "orgeliminar@test.com", EventoId = 1 };
            context.Organizador.Add(organizador);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteOrganizador(organizador.OrganizadorId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var organizadorEnDb = await context.Organizador.FindAsync(organizador.OrganizadorId);
            Assert.Null(organizadorEnDb);
        }




    }
}
