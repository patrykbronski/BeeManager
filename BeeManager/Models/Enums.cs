namespace BeeManager.Models;

public enum RequestedRole
{
    Owner = 1,
    Worker = 2,
    Inspector = 3
}

public enum AccountStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum MembershipRole
{
    Worker = 1,
    Inspector = 2
}

public enum MembershipStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum TypUlaEnum
{
    Wielkopolski = 1,
    Dadant = 2,
    Warszawski = 3
}

public enum StatusUlaEnum
{
    Aktywny = 1,
    Pusty = 2,
    Zniszczony = 3
}

public enum StanRodzinyEnum
{
    BardzoSlaby = 1,
    Slaby = 2,
    Sredni = 3,
    Dobry = 4,
    BardzoDobry = 5
}

public enum TypMioduEnum
{
    Wielokwiatowy = 1,
    Lipowy = 2,
    Akacjowy = 3,
    Gryczany = 4,
    Rzepakowy = 5,
    Spadziowy = 6
}
