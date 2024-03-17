enum code{
    WrtPS,
    GetPS,
    SetPS,
    GetTMP,
    GetId,
    GetVd,
    GetSTB_AMP,
    SetSTB_AMP,
    GetSTB_DRA,
    SetSTB_DRA,
    GetSTB_LNA,
    SetSTB_LNA,
    GetLPM,
    SetLPM,
    SetALD,
    GetALD,
    SaveMEM,
    LoadMEM,
    GetIDN,
    SetIDN,
    RST,
    CUI,
    NONE
};

struct cmd{
    enum code cmd_code;
    int id;
    int arg;
    cmd(enum::code _code, int _id, int _arg){cmd_code=_code;id=_id;arg=_arg;}
};