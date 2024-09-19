using Desafio2APlicacionAPI.Controllers;
using Desafio2APlicacionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionAPi.Test
{
    public class EventoControllerTest
    {
        [Fact]
        public async Task PostEvento_AgregaEvento_CuandoDatosSonValidos()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();

            // Simula la conexión a Redis
            var mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

            // Simula que no pasa nada cuando se borra la clave
            mockDb.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(true);

            // Pasa el mock de Redis al controlador
            var controller = new EventosController(context, mockRedis.Object);
            var nuevoEvento = new Evento { Nombre = "Evento Test", Fecha = DateTime.Now, Lugar = "Lugar Test" };

            // Act
            var result = await controller.PostEvento(nuevoEvento);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var eventoCreado = Assert.IsType<Evento>(actionResult.Value);
            Assert.Equal("Evento Test", eventoCreado.Nombre);
            Assert.Equal("Lugar Test", eventoCreado.Lugar);
        }



        [Fact]
        public async Task GetEvento_RetornaEvento_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new EventosController(context);
            var evento = new Evento { Nombre = "Evento Test", Fecha = DateTime.Now, Lugar = "Lugar Test" };
            context.Evento.Add(evento);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetEventoT(evento.EventoId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Evento>>(result);
            var returnValue = Assert.IsType<Evento>(actionResult.Value);
            Assert.Equal("Evento Test", returnValue.Nombre);  // Verifica que el nombre es correcto
        }

        [Fact]
        public async Task GetEvento_RetornaNotFound_CuandoIdNoExiste()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new EventosController(context);

            // Act
            var result = await controller.GetEventoT(999);  // ID que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);  // Verifica que se retorne NotFound (404)
        }

        [Fact]
        public async Task DeleteEvento_EliminaEvento_CuandoIdEsValido()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new EventosController(context);

            // Agregar un evento al contexto
            var evento = new Evento { Nombre = "Evento Test", Fecha = DateTime.Now, Lugar = "Lugar Test" };
            context.Evento.Add(evento);
            await context.SaveChangesAsync();

            // Verificar que el evento ha sido agregado
            var eventoEnDb = await context.Evento.FindAsync(evento.EventoId);
            Assert.NotNull(eventoEnDb);

            // Act
            var result = await controller.DeleteEventoT(evento.EventoId);

            // Assert
            Assert.IsType<NoContentResult>(result);  // Verificar que el resultado es NoContent (204)

            // Verificar que el evento ha sido eliminado
            var eventoEliminado = await context.Evento.FindAsync(evento.EventoId);
            Assert.Null(eventoEliminado);  // Verificar que ya no existe en la base de datos
        }
        [Fact]
        public async Task DeleteEvento_RetornaNotFound_CuandoIdNoExiste()
        {
            // Arrange
            var context = SetUp.GetInMemoryDatabaseContext();
            var controller = new EventosController(context);

            // Act
            var result = await controller.DeleteEventoT(999);  // ID que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result);  // Verificar que el resultado es NotFound (404)
        }

    }
}
