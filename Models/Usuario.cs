using System;
using System.Collections.Generic;

namespace Api_Usuario.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Region { get; set; } = null!;

    public string ClaveUsuario { get; set; } = null!;

    public string? Uid { get; set; }
}
