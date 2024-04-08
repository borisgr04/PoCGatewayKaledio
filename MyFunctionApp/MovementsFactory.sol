pragma solidity ^0.5.0;

contract MovementsFactory  {
    mapping(string => string) Transactions;
    mapping(string => Movement) Movements;

     uint8 eventType;

    enum ActionType { Insert, Update, Delete }

    event TrueLog(
        int64 indexed Type,
        int64 indexed Version,
        bytes32 indexed Id,
        string Data,
        uint Timestamp
    );

    event TrueMovementLog(
        bytes32 indexed Id,
        bytes32 indexed MovementId,
        int64 indexed OperationalDate,
        uint8 Version,
        int256 Volume,
        uint8 ActionType,
        string Unit,
        string Metadata,
        uint Timestamp
    );

    struct Movement {
        string MovementId;
        int256 NetStandardVolume;
        string MeasurementUnit;
        int64  OperationalDate;
        string TransactionId;
        string Metadata;
        bool Exists;
    }

    constructor() public {}

    function Insert(
        string calldata movementId,
        int256 netStandardVolume,
        string calldata transactionId,
        int64 operationalDate,
        string calldata measurementUnit,
        string calldata metadata
    )
    external {
        string memory existingTransactionId = Transactions[movementId];
        require(compareStrings(existingTransactionId, ""), "Movement identifier is already present");

        Transactions[movementId] = transactionId;
        Movements[transactionId] = Movement(movementId, netStandardVolume, measurementUnit,
                                           operationalDate, transactionId, metadata, true);

        bytes32 id = keccak256(bytes(transactionId));

        emit TrueLog(GetEventType(), 2, id, "", now);
        emit TrueMovementLog(id, keccak256(bytes(movementId)), operationalDate, 2,
                        netStandardVolume, GetInsertType(), measurementUnit, metadata, now);
    }

    function Update(
        string calldata movementId,
        int256 netStandardVolume,
        string calldata transactionId,
        int64 operationalDate,
        string calldata measurementUnit,
        string calldata metadata
    )
    external {
        string memory existingTransactionId = Transactions[movementId];
        require(!compareStrings(existingTransactionId, ""), "Movement identifier must be present");
        require(Movements[transactionId].Exists == false, "Movement transaction identifier must be unique");

        delete Movements[existingTransactionId];
        Transactions[movementId] = transactionId;
        Movements[transactionId] = Movement(movementId, netStandardVolume, measurementUnit,
                                         operationalDate, transactionId, metadata, true);
        bytes32 id = keccak256(bytes(transactionId));

        emit TrueLog(GetEventType(), 2, id, "", now);
        emit TrueMovementLog(id, keccak256(bytes(movementId)), operationalDate, 2,
                        netStandardVolume, GetUpdateType(), measurementUnit, metadata, now);
    }

    function Delete(
        string calldata movementId,
        string calldata transactionId,
        int64 operationalDate,
        string calldata metadata
    )
    external {
        string memory existingTransactionId = Transactions[movementId];
        require(!compareStrings(existingTransactionId, ""), "Movement identifier must be present");
        require(Movements[transactionId].Exists == false, "Movement transaction identifier must be unique");

        delete Transactions[movementId];
        delete Movements[existingTransactionId];

        bytes32 id = keccak256(bytes(transactionId));

        emit TrueLog(GetEventType(), 2, id, "", now);
        emit TrueMovementLog(id, keccak256(bytes(movementId)), operationalDate, 2, 0, GetDeleteType(), "", metadata, now);
    }

    function GetByTransactionId(
        string memory blockchainMovementTransationId
    )
        public
        view
        returns (
            string memory MovementId,
            int256 NetStandardVolume,
            string memory MeasurementUnit,
            int64 OperationalDate,
            string memory TransactionId,
            string memory Metadata,
            bool Exists
        )
    {
        Movement memory movement = Movements[blockchainMovementTransationId];
        return (
            movement.MovementId,
            movement.NetStandardVolume,
            movement.MeasurementUnit,
            movement.OperationalDate,
            movement.TransactionId,
            movement.Metadata,
            movement.Exists
        );
    }

    function GetByMovementId(string memory movementId)
        public
        view
        returns (
            string memory MovementId,
            int256 NetStandardVolume,
            string memory MeasurementUnit,
            int64 OperationalDate,
            string memory TransactionId,
            string memory Metadata,
            bool Exists
        )
    {
        string memory transactionId = Transactions[movementId];
        Movement memory movement = Movements[transactionId];
        return (
            movement.MovementId,
            movement.NetStandardVolume,
            movement.MeasurementUnit,
            movement.OperationalDate,
            movement.TransactionId,
            movement.Metadata,
            movement.Exists
        );
    }

    function compareStrings(string memory a, string memory b) internal pure returns (bool){
        return keccak256(abi.encodePacked(a)) == keccak256(abi.encodePacked(b));
    }

    function GetEventType() internal view returns (uint8) {
        return 1;
    }

    function GetInsertType() internal pure returns (uint8) {
        return uint8(ActionType.Insert);
    }

    function GetUpdateType() internal pure returns (uint8) {
        return uint8(ActionType.Update);
    }

    function GetDeleteType() internal pure returns (uint8) {
        return uint8(ActionType.Delete);
    }
}